﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReDesign.Entities
{
    public class Player : Entity
    {
        [SerializeField] private ManaSystem _manaSystem;
        private List<AttacksAndSpells> _attacks = new List<AttacksAndSpells>
        {
            new BasicFireSpell(),
            new BasicIceSpell()
        };

        private void Awake()
        {
            int MaxHealth = 20;
            _entityHealth = new UnitHealth(MaxHealth, MaxHealth);
        }

        public override void NextAction()
        {
            StateController.ChangeState(GameState.PlayerTurn);
            Debug.Log("im a player");
            //StateController.ChangeState(GameState.EndTurn);
            turnShouldEnd = false;
            _manaSystem.StartTurn();
        }

        public override void Move()
        {
            //throw new System.NotImplementedException();
        }

        public override void Attack()
        {
            attacking = false;
        }

        public void EndTurn() => turnShouldEnd = true;
    }
}