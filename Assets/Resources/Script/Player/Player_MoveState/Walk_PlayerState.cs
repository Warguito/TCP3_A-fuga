using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk_PlayerState : Player_StateMachine
{
    public override void EnterState(Player_Controller player)
    {

    }

    public override void UpdateState(Player_Controller player)
    {
        if (player.airJumpCount != player.maxAirJumpCount) player.airJumpCount = player.maxAirJumpCount;

        if (Input.GetKeyDown(KeyCode.Space))
            Jump(player);

        if (Input.GetKeyDown(KeyCode.LeftShift) && player.dashCoolDown < 0)
            PlayerDash(player);
        else
            player.dashCoolDown -= Time.deltaTime;

        Move(player);


        ChangeState(player);
    }

    public void Move(Player_Controller player)
    {
        player.moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        if (player.moveDirection.magnitude >= 0.1f)
        {
            if (player.stepSoundDelay <= 0) player.PlayStepFX();
            else player.stepSoundDelay -= Time.deltaTime;

            float targetAngle = Mathf.Atan2(player.moveDirection.x, player.moveDirection.z) * Mathf.Rad2Deg + player.cameraTransform.eulerAngles.y;

            float angle = Mathf.SmoothDampAngle(player.transform.eulerAngles.y, targetAngle, ref player.turnSmoothVelocity, player.turnSmoothTime);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * player.playerSpeed;

            moveDir.y = player.playerRB.velocity.y;

            player.gooseAnimator.SetBool("Runnig", true);

            if (Mathf.Abs(player.playerRB.velocity.x) + Mathf.Abs(player.playerRB.velocity.z) <= 7.5f)
            {
                player.playerRB.velocity = moveDir;
                player.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            else
                this.Friction(player);
        }
        else
            this.Friction(player);
    }

    public void Jump(Player_Controller player)
    {
        player.gooseAnimator.SetTrigger("Pular");
        player.audioSource.PlayOneShot(player.audioClipJump, 0.8f);
        player.playerRB.velocity = new Vector3(player.playerRB.velocity.x, player.playerJumpForce, player.playerRB.velocity.z);
    }

    public void PlayerDash(Player_Controller player)
    {
        player.gooseAnimator.SetTrigger("Dash");
        player.playerRB.AddForce(player.playerRB.transform.forward * 400);
        player.dashCoolDown = 1.5f;
    }

    public void ChangeState(Player_Controller player)
    {
        if (!player.onGoundInstance.isOnGround)
            player.ChangeState(player.air_PlayerState);

        if (player.IsRagdollEffect())
            player.ChangeState(player.ragDoll_PlayerState);
    }

    public void Friction(Player_Controller player)
    {
        Vector3 playerActualSpeed = player.playerRB.velocity;

        if (Mathf.Abs(playerActualSpeed.x) > 5)
            playerActualSpeed.x -= Mathf.Sign(playerActualSpeed.x) * player.frictionValue * Time.deltaTime;
        else
            playerActualSpeed.x = 0;

        if (Mathf.Abs(playerActualSpeed.z) > 5)
            playerActualSpeed.z -= Mathf.Sign(playerActualSpeed.z) * player.frictionValue * Time.deltaTime;
        else
            playerActualSpeed.z = 0;

        if(playerActualSpeed.z + playerActualSpeed.x == 0)
            player.gooseAnimator.SetBool("Runnig", false);


        player.playerRB.velocity = playerActualSpeed;
    }
}
