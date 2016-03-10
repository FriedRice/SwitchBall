﻿using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class FireBullet : MonoBehaviour {
    const KeyCode FIRE_LEFT_KEY = KeyCode.LeftArrow;
    const KeyCode FIRE_DOWN_KEY = KeyCode.DownArrow;
    const KeyCode FIRE_RIGHT_KEY = KeyCode.RightArrow;
    const KeyCode FIRE_UP_KEY = KeyCode.UpArrow;
    const KeyCode SWAP_ATTACK_KEY = KeyCode.W;

    public float fire_bullet_delay = 0.3f;
    public float swap_attack_cooldown = 3.0f;
    public GameObject swap_attack_cooldown_bar_go;
    CooldownBar swap_attack_cooldown_bar;

    float stop_bullet_fire_time;
    float swap_attack_cooldown_start;
    bool fired_swap;
    int player;

    Player player_comp;
    PlayerManager player_manager_comp;
    Object bullet_prefab;
    Object swap_attack_prefab;
    Vector2 fire_bullet_vector;
    Coroutine fire_bullets;
    GamePadState pad_state;

    // Use this for initialization
    void Start() {
        fire_bullets = null;
        bullet_prefab = Resources.Load("Bullet");
        swap_attack_prefab = Resources.Load("Swap");

        stop_bullet_fire_time = -fire_bullet_delay;
        swap_attack_cooldown_start = -swap_attack_cooldown;
        swap_attack_cooldown_bar = swap_attack_cooldown_bar_go.GetComponent<CooldownBar>();

        player_comp = this.GetComponent<Player>();
        player_manager_comp = this.GetComponent<PlayerManager>();
        player = player_comp.player;
    }

    // Update is called once per frame
    void Update() {
        if (SystemInfo.operatingSystem.StartsWith("Windows")) {
            pad_state = GamePad.GetState((PlayerIndex)player);
        }
        if (this.GetComponent<PlayerManager>().death == true) {
            return;
        }
        updateFireDirection();
        updateSwapAttack();
    }

    void updateFireDirection() {
        fire_bullet_vector = Vector2.zero;
        //controller bullet fire
        float x_look_val;
        float y_look_val;
        bool fire_delay_exceeded = (Time.time - stop_bullet_fire_time) >= fire_bullet_delay;
			x_look_val = pad_state.ThumbSticks.Right.X;
			y_look_val = pad_state.ThumbSticks.Right.Y;
		if (player_comp.useController) {
			fire_bullet_vector.x += x_look_val;
			fire_bullet_vector.y += y_look_val;
		} else if (player_comp.macController) {
			x_look_val = Input.GetAxis (Controls.axes_codes [player, Controls.axis_right_joy_hor]);
			y_look_val = Input.GetAxis (Controls.axes_codes [player, Controls.axis_right_joy_vert]);
		}
		else {
            if (Input.GetKey(FIRE_LEFT_KEY)) {
                fire_bullet_vector.x -= 1;
            }
            if (Input.GetKey(FIRE_DOWN_KEY)) {
                fire_bullet_vector.y -= 1;
            }
            if (Input.GetKey(FIRE_RIGHT_KEY)) {
                fire_bullet_vector.x += 1;
            }
            if (Input.GetKey(FIRE_UP_KEY)) {
                fire_bullet_vector.y += 1;
            }
        }
        fire_bullet_vector = fire_bullet_vector.normalized;

        if (fire_bullet_vector != Vector2.zero && fire_bullets == null && fire_delay_exceeded &&
                OddBall.Instance.BelongTo != player_manager_comp) {
            fire_bullets = StartCoroutine(fireBullets());
        } else if (fire_bullets != null &&
                (fire_bullet_vector == Vector2.zero || OddBall.Instance.BelongTo == player_manager_comp)) {
            StopCoroutine(fire_bullets);
            fire_bullets = null;
            stop_bullet_fire_time = Time.time;
        }
    }

    IEnumerator fireBullets() {
        while (true) {
            GameObject new_bullet = Instantiate(bullet_prefab) as GameObject;
            new_bullet.GetComponent<Bullet>().source_player_id = this.GetComponent<PlayerManager>().player_id;
            new_bullet.GetComponent<Bullet>().movement_vector = fire_bullet_vector;
            new_bullet.transform.position = this.transform.position;
            new_bullet.GetComponent<Bullet>().bullet_team = this.GetComponent<PlayerManager>().Team;
            yield return new WaitForSeconds(fire_bullet_delay);
        }
    }

    void updateSwapAttack() {
        float cooldown_progress = (Time.time - swap_attack_cooldown_start) / swap_attack_cooldown;
        swap_attack_cooldown_bar.GetComponent<CooldownBar>().setProgress(Mathf.Min(cooldown_progress, 1f));
		if (player_comp.macController) {
			if ((!Input.GetKey (SWAP_ATTACK_KEY) &&
			    !(Input.GetAxis (Controls.axes_codes [player, Controls.axis_left_trigger]) > 0.0f)) ||
			    cooldown_progress < 1.0f || fire_bullet_vector == Vector2.zero) {
				return;
			}
		}
		else if ((!Input.GetKey(SWAP_ATTACK_KEY) && !(pad_state.Triggers.Left > 0.0f)) ||
				cooldown_progress < 1.0f || fire_bullet_vector == Vector2.zero) {
            	return;
        }

        GameObject new_swap_attack = Instantiate(swap_attack_prefab) as GameObject;
        new_swap_attack.GetComponent<SwapAttack>().source_player_id = this.GetComponent<PlayerManager>().player_id;
        new_swap_attack.GetComponent<SwapAttack>().movement_vector = fire_bullet_vector;
        new_swap_attack.transform.position = this.transform.position;
        new_swap_attack.GetComponent<SwapAttack>().bullet_team = this.GetComponent<PlayerManager>().Team;
        swap_attack_cooldown_start = Time.time;
    }
}
