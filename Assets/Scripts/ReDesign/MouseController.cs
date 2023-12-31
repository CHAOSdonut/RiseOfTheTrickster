﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CombatMenu;
using ReDesign.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace ReDesign
{
    public class MouseController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private StatusBar manaSystem;
        [field:SerializeField] public Tilemap SelectorMap{get; private set;}
        [SerializeField] public Tilemap GridMap;

        private static MouseController _instance;
        private static bool drawSelectedTile = true;
        private Vector3 targetLocation;

        public static MouseController Instance
        {
            get { return _instance; }
        }

        public static AttacksAndSpells spellSelection = null;
        public ParticleSystem fireParticles;
        [SerializeField] private AudioClip fireSound;
        public ParticleSystem iceParticles;
        public ParticleSystem waterParticles;
        [SerializeField] private AudioClip iceSound;
        private DefaultTile prevSelectedTile;
        [SerializeField] private SpellMenu spellMenu;
        private BasicFireSpell fireSpell;
        private BasicIceSpell iceSpell;
        private BasicWaterSpell waterSpell;        
        [SerializeField] private AudioClip waterSound;
        private Canvas pauseMenu;
        private ActionButton movementButton;
        private Canvas helpScreen;
        private Canvas _tutorialCanvas;
        private SpellBar _spellBar;

        private void Awake()
        {
            fireSpell = new BasicFireSpell(fireParticles);
            iceSpell = new BasicIceSpell(iceParticles);
            waterSpell = new BasicWaterSpell(waterParticles);

            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }

            pauseMenu = GameObject.Find("PauseMenu").GetComponent<Canvas>();
            helpScreen = GameObject.Find("HelpScreen").GetComponent<Canvas>();
            movementButton = GameObject.Find("MovementButton").GetComponent<ActionButton>();
            _spellBar = GameObject.Find("Spellbar").GetComponent<SpellBar>();
        }

        private void Update()
        {
            Vector3 mousePosition = GetMouseWorldPos();
            DefaultTile selectedTile = MouseToTile(mousePosition);
            if (pauseMenu.enabled || helpScreen.enabled || PlayerAnimator._animator.GetBool("isWalking")){
                return;
            }

            if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                _tutorialCanvas = GameObject.Find("Tutorial").GetComponent<Canvas>();
                if (_tutorialCanvas.enabled)
                {
                    return;
                }
            }


            List<DefaultTile> pathNodesMap = WorldController.Instance.BaseLayer;

            if (selectedTile != prevSelectedTile)
            {
                GridLayout gr = WorldController.Instance.gridLayout;
                player.ShowPath(mousePosition, gr, pathNodesMap);
                DrawCurrentSelectedTile(selectedTile);
                DrawCurrentSpellRange();
            }

            prevSelectedTile = selectedTile;
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (spellSelection == null)
            {
                GridLayout gr = WorldController.Instance.gridLayout;
                player.MovePlayer(mousePosition, gr, pathNodesMap);
            }
            else if(!_spellBar.MouseOver){
                int playerPosX = player.FindNearestXYPathNode(player.gameObject.transform.position, pathNodesMap).XPos;
                int playerPosY = player.FindNearestXYPathNode(player.gameObject.transform.position, pathNodesMap).YPos;
                if (spellSelection.GetTargetLocations(playerPosX, playerPosY)
                        .Contains(player.FindNearestXYPathNode(mousePosition, pathNodesMap)) &&
                    manaSystem.Value >= spellSelection.ManaCost)
                {
                    DefaultTile nearestPathNode = player.FindNearestXYPathNode(mousePosition, pathNodesMap);
                    int x = nearestPathNode.XPos;
                    int y = nearestPathNode.YPos;
                    StartCoroutine(Player.RotateToAttack());
                    CheckSpellCasted(spellSelection);
                    spellSelection.Effect(x, y);
                    manaSystem.Value -= spellSelection.ManaCost;
                    spellMenu.AllowedToOpen = false;
                }
                else
                {
                    movementButton.Activate();
                }

                spellMenu.Close();
                spellSelection = null;
                RangeTileTool.Instance.clearTileMap(SelectorMap);
                CheckSpellCasted(spellSelection);

                StopCoroutine(Player.RotateToAttack());
            }
        }

        public static Vector3 GetMouseWorldPos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                drawSelectedTile = true;
                return raycastHit.point;
            }
            else
            {
                drawSelectedTile = false;
                return Vector3.zero;
            }
        }

        public DefaultTile MouseToTile(Vector3 mousePosition)
        {
            DefaultTile hoveredNode = WorldController.Instance.BaseLayer
                .OrderBy(item => Math.Abs(mousePosition.x - item.GameObject.transform.position.x))
                .ThenBy(item => Math.Abs(mousePosition.z - item.GameObject.transform.position.z)).ToList()
                .FirstOrDefault();

            return hoveredNode;
        }

        void SelectSpell(AttacksAndSpells spell){
            RangeTileTool.Instance.clearTileMap(RangeTileTool.Instance.rangeTileMap);
            if(spell.ManaCost <= manaSystem.Value){
                spellSelection = spell;
                DrawCurrentSelectedTile(MouseToTile(GetMouseWorldPos()));
                DrawCurrentSpellRange();
            }
            else{
                spellSelection = null;
            }
        }

        public void SelectFireSpell() => SelectSpell(fireSpell);

        public void SelectIceSpell() => SelectSpell(iceSpell);
        
        public void SelectWaterSpell() => SelectSpell(waterSpell);
        
        public void DeselectSpell() => spellSelection = null;

        private void DrawCurrentSelectedTile(DefaultTile hoveredNode)
        {
            Color color = new Color(255, 255, 255, 0.05f);
            prevSelectedTile = hoveredNode;
            RangeTileTool.Instance.clearTileMap(GridMap);
            RangeTileTool.Instance.clearTileMap(SelectorMap);
            if (hoveredNode != null && drawSelectedTile)
            {
                RangeTileTool.Instance.SpawnTile(hoveredNode.XPos, hoveredNode.YPos, color, GridMap, false);
            }
        }

        private void DrawCurrentSpellRange()
        {
            if (spellSelection != null)
            {
                DefaultTile playerPos = player.FindNearestXYPathNode(player.gameObject.transform.position,
                    WorldController.Instance.BaseLayer);
                List<DefaultTile> targets = spellSelection.GetTargetLocations(playerPos.XPos, playerPos.YPos);

                foreach (var t in targets)
                {
                    RangeTileTool.Instance.SpawnTile(t.XPos, t.YPos, new Color(0, 0, 255, 0.2f), SelectorMap, false);
                }
            }
        }

        private void CheckSpellCasted(AttacksAndSpells spellSelection)
        {
            if (spellSelection != null)
            {
                if (spellSelection.GetType() == typeof(BasicFireSpell))
                {
                    PlayerAnimator._animator.SetBool("fireCasted", true);
                    SoundManager.Instance.PlaySound(fireSound);
                }
                else if (spellSelection.GetType() == typeof(BasicIceSpell))
                {
                    PlayerAnimator._animator.SetBool("iceCasted", true);
                    SoundManager.Instance.PlaySound(iceSound);

                }

                if (spellSelection.GetType() == typeof(BasicWaterSpell))
                {
                    PlayerAnimator._animator.SetBool("iceCasted", true);
                    SoundManager.Instance.PlaySound(waterSound);
                }
            }
        }


    }
}