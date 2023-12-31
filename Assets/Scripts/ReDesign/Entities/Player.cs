﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReDesign.Entities
{
    public class Player : Entity
    {
        private static Transform player;
        private static Vector3 targetLocation;
        [SerializeField] private StatusBar _manaSystem;
        [SerializeField] private StatusBar _healthBar;
        [SerializeField] private ActionButton movementButton;
        public static Animator _animator;
        public override string displayName{get{ return "Loki"; }}
        
        public override int SightRange { get; }
        public override int MoveRange { get { return _manaSystem.Value; } }
        [SerializeField] private AudioClip dmgClip;
        [SerializeField] private AudioClip deathClip;

        
        public virtual void Awake()
        {
            _animator = GetComponent<Animator>();
            int MaxHealth = 20;
            _entityHealth = new UnitHealth(MaxHealth, MaxHealth);
            player = transform;
            _healthBar.maxValue = MaxHealth;
        }

        public override void Start()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
            RangeTileTool.Instance.drawMoveRange(WorldController.getPlayerTile(), _manaSystem.Value);
            _healthBar.Fill();
        }

        public override void Update()
        {
            if (_animator.GetBool("hasCasted") && MouseController.spellSelection == null)
            {
                RangeTileTool.Instance.drawMoveRange(WorldController.getPlayerTile(), _manaSystem.Value);
                _animator.SetBool("hasCasted", false);
            }
        }

        public override void NextAction()
        {
            StateController.ChangeState(GameState.PlayerTurn);
            _manaSystem.Fill();
            RangeTileTool.Instance.drawMoveRange(WorldController.getPlayerTile(), _manaSystem.Value);
            movementButton.Activate();
        }

            
        public override void Move()
        {
            //throw new System.NotImplementedException();
        }

        public override void Attack(AudioClip attackSound)
        {

        }

        public void EndTurn(){
            if(StateController.currentState == GameState.PlayerTurn){
                StateController.ChangeState(GameState.EndTurn);
            }
        }

        public override void ReceiveDamage(int dmg)
        {
            _entityHealth.ChangeHealth(-dmg);
            _healthBar.Value = _entityHealth.Health;
            SoundManager.Instance.PlaySound(dmgClip);

            PlayerAnimator._animator.SetBool("isHit", true);
            if (_entityHealth.Health <= 0){
                TurnController.gameOver = true;
                PlayerAnimator._animator.SetBool("PlayerDead", true);                
                SoundManager.Instance.PlaySound(deathClip);
            }
            TurnController.Instance.gameOverEvent.Invoke();
        }

        public static IEnumerator RotateToAttack()
        {
            Vector3 attackerPos = player.transform.position;
            Vector3 targetPos = MouseController.GetMouseWorldPos();
            GridLayout gr = WorldController.Instance.gridLayout;
            // Calculate the direction to the target position and set the entity's rotation accordingly
            Vector3 targetPosition = new Vector3(targetPos.x, attackerPos.y, targetPos.z);
            Vector3 dir = (targetPosition - attackerPos).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
            targetLocation = PlayerMovement.SnapCoordinateToGrid(targetPos, gr);
            float time = 0;

            // Loop until the entity has moved halfway to the target location
            while (time < 0.5f)
            {
                // Adds the position and rotation
                player.transform.rotation = Quaternion.Lerp(player.transform.rotation, targetRotation, time / 0.5f);
                time += Time.deltaTime;
                yield return null;
            }
            player.transform.rotation = targetRotation;
        }
    }
}