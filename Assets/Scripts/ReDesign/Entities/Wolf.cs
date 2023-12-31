using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReDesign.Entities
{
    public class Wolf : Enemy
    {
        public override int SightRange { get { return 12; } }
        public override int MoveRange { get { return 4; } }
        public override string displayName{get{ return "Direwolf"; }}

        private EntityAnimator _wolfAnimator;
        public Wolf()
        {
            int MaxHealth = 8;
            _entityHealth = new UnitHealth(MaxHealth, MaxHealth);
            Attacks = new List<AttacksAndSpells>()
            {
                new WolfAttack()
            };
        }

        private void Awake()
        {
            _wolfAnimator = GetComponentInChildren<EntityAnimator>();
        }

        public override void NextAction()
        {
            //Debug.Log("im a Wolf");
            StateController.ChangeState(GameState.EnemyTurn);

            //Move() will call Attack() and change turn
            Move();
            
        }

        public override void Move()
        {
            DefaultTile currentTile = WorldController.ObstacleLayer.Where(o => o.GameObject == this.gameObject).FirstOrDefault();
            DefaultTile enemyPos = WorldController.getPlayerTile();
            int range = Math.Abs(currentTile.XPos - enemyPos.XPos) + Math.Abs(currentTile.YPos - enemyPos.YPos);
            if (range < SightRange)
            {
                MoveToPlayer(this.MoveRange, _wolfAnimator);
            } else
            {
                //MoveToPlayer(0);
                if (SceneManager.GetActiveScene().name == "Level3Map"){
			        MoveToObject(this.MoveRange, _wolfAnimator, "WallTrigger");
		        } else {
                	MoveToPlayer(0);
		        }
            }

            //foreach(AttacksAndSpells atk in _attacks)
        }

        public override void Attack(AudioClip attackSound)
        {
            DefaultTile currentTile = WorldController.ObstacleLayer.Where(o => o.GameObject == this.gameObject).FirstOrDefault();
            List<DefaultTile> targetTiles = Attacks[0].GetTargetLocations(currentTile.XPos, currentTile.YPos);
            DefaultTile targetTile = targetTiles.Where(t => t.XPos == WorldController.getPlayerTile().XPos && t.YPos == WorldController.getPlayerTile().YPos).FirstOrDefault();
            if (targetTile != null)
            {
                _wolfAnimator.SetAttacking();
                SoundManager.Instance.PlaySound(attackSound);

                StartCoroutine(EnemyRotateToAttack());
                Attacks[0].Effect(targetTile.XPos, targetTile.YPos);
            }

            // if (wolfAnimator._animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            // {
            //     wolfAnimator._animator.SetBool("IsAttacking", false);
            // }

            StopCoroutine(EnemyRotateToAttack());
        }
    }
}