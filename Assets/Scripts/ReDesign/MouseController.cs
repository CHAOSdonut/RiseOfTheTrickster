﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ReDesign.Entities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ReDesign
{
    public class MouseController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement player;
        [SerializeField] private StatusBar manaSystem;
        [SerializeField] private Tilemap SelectorMap;
        private static MouseController _instance;
        private static bool drawSelectedTile = true;
        private Vector3 targetLocation;
        public static MouseController Instance { get { return _instance; } }
        private AttacksAndSpells spellSelection = null;
        public ParticleSystem fireParticles;
        public ParticleSystem iceParticles;
        private DefaultTile prevSelectedTile;
        [Header("Tile info")]
        [SerializeField] private TMPro.TMP_Text tileTitleText;
        [SerializeField] private TMPro.TMP_Text tileHpText;
        [SerializeField] private TMPro.TMP_Text tileDistanceText;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }
        }
        private void Update()
        {
            DrawCurrentSelectedTile();
            
            DrawCurrentSpellRange();

            if (Input.GetMouseButtonDown(1))
            {
                ShowEntityInfo();
            }
            
            List<DefaultTile> pathNodesMap = WorldController.Instance.BaseLayer;
            {
                Vector3 pos = GetMouseWorldPos();
                GridLayout gr = WorldController.Instance.gridLayout;
                player.ShowPath(pos, gr, pathNodesMap);
            }
            if(!Input.GetMouseButtonDown(0)){
                return;
            }
            if (spellSelection == null)
            {
                Vector3 pos = GetMouseWorldPos();
                GridLayout gr = WorldController.Instance.gridLayout;
                player.MovePlayer(pos, gr, pathNodesMap);
            }else{
                int playerPosX = player.FindNearestXYPathNode(player.gameObject.transform.position, pathNodesMap).XPos;
                int playerPosY = player.FindNearestXYPathNode(player.gameObject.transform.position, pathNodesMap).YPos;
                if (spellSelection.GetTargetLocations(playerPosX, playerPosY).Contains(player.FindNearestXYPathNode(GetMouseWorldPos(), pathNodesMap)) && manaSystem.Value >= spellSelection.ManaCost)
                {
                    int x = player.FindNearestXYPathNode(GetMouseWorldPos(), pathNodesMap).XPos;
                    int y = player.FindNearestXYPathNode(GetMouseWorldPos(), pathNodesMap).YPos;
                    StartCoroutine(Player.RotateToAttack());    
                    CheckSpellCasted(spellSelection);
                    spellSelection.Effect(x, y);
                    manaSystem.Value -= spellSelection.ManaCost;
                }
                spellSelection = null;
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

        public DefaultTile MouseToTile()
        {
            DefaultTile hoveredNode = WorldController.Instance.BaseLayer
                .OrderBy(item => Math.Abs(GetMouseWorldPos().x - item.GameObject.transform.position.x))
                .ThenBy(item => Math.Abs(GetMouseWorldPos().z - item.GameObject.transform.position.z)).ToList()
                .FirstOrDefault();

            return hoveredNode;
        }
        
        public void SelectFireSpell(){
            spellSelection = new BasicFireSpell();
            spellSelection.particleSystem = fireParticles;
        }

        public void SelectIceSpell(){
            spellSelection = new BasicIceSpell();
            spellSelection.particleSystem = iceParticles;
        }
        
        private void DrawCurrentSelectedTile()
        {
            if (MouseToTile() != prevSelectedTile)
            {
                Color color = new Color(255, 255, 255, 0.05f);
                DefaultTile hoveredNode = MouseToTile();
                prevSelectedTile = hoveredNode;
                RangeTileTool.Instance.clearTileMap(SelectorMap);
                if (hoveredNode != null && drawSelectedTile)
                {
                    RangeTileTool.Instance.SpawnTile(hoveredNode.XPos, hoveredNode.YPos, color, SelectorMap, false);
                }
            }
        }

        private void DrawCurrentSpellRange()
        {
            if (spellSelection != null)
            {
                DefaultTile playerPos = player.FindNearestXYPathNode(player.gameObject.transform.position, WorldController.Instance.BaseLayer);
                List<DefaultTile> targets = spellSelection.GetTargetLocations(playerPos.XPos, playerPos.YPos);

                foreach (var t in targets)
                {
                    RangeTileTool.Instance.SpawnTile(t.XPos,t.YPos, new Color(255,0,0,0.5f), SelectorMap, false);
                }
            }
        }
        
        private static void CheckSpellCasted(AttacksAndSpells spellSelection)
        {
            if (spellSelection != null)
            {
                if (spellSelection.GetType() == typeof(BasicFireSpell)) PlayerAnimator._animator.SetBool("fireCasted", true);

                if (spellSelection.GetType() == typeof(BasicIceSpell)) PlayerAnimator._animator.SetBool("iceCasted", true);
            }
        }

        private void ShowEntityInfo()
        {
            //comment, like and subscribe
            GameObject enemyTile = WorldController.ObstacleLayer.FirstOrDefault(t => t.XPos == MouseToTile().XPos && t.YPos == MouseToTile().YPos)?.GameObject;

            if (enemyTile != null && enemyTile.CompareTag("Entity"))
            {
                Entity entity = enemyTile.GetComponent<Entity>();
                DefaultTile tile = MouseToTile();
                RangeTileTool.Instance.drawMoveRange(tile, entity.MoveRange);
                tileTitleText.text = entity.name;
                tileHpText.text = "HP: " + entity._entityHealth.Health.ToString();
                tileDistanceText.text = "Distance: " + Mathf.RoundToInt(Vector3.Distance(player.transform.position, tile.GameObject.transform.position));
            }
        }
    }
}