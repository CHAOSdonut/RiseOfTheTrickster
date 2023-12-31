﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;


namespace ReDesign.Entities
{
    public abstract class Entity : MonoBehaviour
    {
        public UnitHealth _entityHealth { get; set; }
        public List<AttacksAndSpells> Attacks { get; set; }
        public bool finishedMoving = false;
        private Vector3 targetLocation;
        public IEnumerator movingCoroutine;
        private static GameObject _gameOver;
        private Vector3 targetLoc;
        public abstract void NextAction();
        public abstract void Move();
        public abstract void Attack(AudioClip attackSound);
        public abstract int SightRange { get; }
        public abstract int MoveRange { get; }
        private GameObject healthBarObj;
        private Camera cam;
        [SerializeField] private AudioClip walkingClip;
        [SerializeField] private AudioClip attackingClip;
        public abstract string displayName{get;}
        

        public virtual void Start()
        {
            cam = Camera.main;
            healthBarObj = getChildGameObject(gameObject, "HealthBar");
        }

        private void LateUpdate()
        {
            if (healthBarObj)
            {
                healthBarObj.transform.LookAt(healthBarObj.transform.position + cam.transform.rotation * Vector3.forward,cam.transform.rotation * Vector3.up); 
            }
        }

        public abstract void ReceiveDamage(int dmg);

        public void MoveToPlayer(int movementRange, EntityAnimator animator = null)
        {
            DefaultTile currentTile = WorldController.ObstacleLayer
                .FirstOrDefault(o => o.GameObject == this.gameObject);
            List<DefaultTile> targetLocations = Attacks[0].GetTargetLocations(currentTile.XPos, currentTile.YPos);
            DefaultTile enemyPos = WorldController.getPlayerTile();
            if (targetLocations.FirstOrDefault(t => t.XPos == enemyPos.XPos && t.YPos == enemyPos.YPos) == null)
            {
                int widthAndHeight = (int)Mathf.Sqrt(WorldController.Instance.BaseLayer.Count);
                PlayerPathfinding pf =
                    new PlayerPathfinding(widthAndHeight, widthAndHeight, WorldController.Instance.BaseLayer);

                List<DefaultTile> path = null;

                foreach (DefaultTile dt in pf.GetNeighbourList(enemyPos))
                {
                    List<DefaultTile> newPath = pf.FindPath(currentTile.XPos, currentTile.YPos, dt.XPos, dt.YPos);
                    if (newPath != null && (path == null || newPath.Count < path.Count))
                    {
                        path = newPath;
                    }
                }
                
                if (path != null)
                {
                    List<DefaultTile> actualPath = new List<DefaultTile>();
                    if(movementRange >= path.Count){
                        if (path.Count > 0){
                            movementRange = path.Count-1;
                        } else {
                            movementRange = 0;
                        }
                    }
                    actualPath.AddRange(path.GetRange(0, movementRange+1));

                    actualPath.First().Walkable = true;
                    actualPath.Last().Walkable = false;


                    movingCoroutine = EntityMoveSquares(actualPath, animator);
                    StartCoroutine(movingCoroutine);
                    currentTile.XPos = actualPath.Last().XPos;
                    currentTile.YPos = actualPath.Last().YPos;
                }
                else
                {
                    finishedMoving = true;
                }
            }
            else
            {
                finishedMoving = true;
            }
        }

        public void MoveToObject(int movementRange, EntityAnimator animator = null, string name = "")
        {
            DefaultTile currentTile = WorldController.ObstacleLayer
                .FirstOrDefault(o => o.GameObject == this.gameObject);
            List<DefaultTile> targetLocations = Attacks[0].GetTargetLocations(currentTile.XPos, currentTile.YPos);
            DefaultTile enemyPos = WorldController.getPlayerTile();
            foreach (var tile in WorldController.ObstacleLayer)
            {
                if (tile.GameObject.name.Equals(name))
                {
                    enemyPos = tile;
                }
            }
            if (targetLocations.FirstOrDefault(t => t.XPos == enemyPos.XPos && t.YPos == currentTile.YPos) == null)
            {
                int widthAndHeight = (int)Mathf.Sqrt(WorldController.Instance.BaseLayer.Count);
                PlayerPathfinding pf =
                    new PlayerPathfinding(widthAndHeight, widthAndHeight, WorldController.Instance.BaseLayer);

                List<DefaultTile> path = null;

                List<DefaultTile> newPath = pf.FindPath(currentTile.XPos, currentTile.YPos, enemyPos.XPos, currentTile.YPos);
                if (newPath != null && (path == null || newPath.Count < path.Count))
                {
                    path = newPath;
                }
                
                if (path != null)
                {
                    List<DefaultTile> actualPath = new List<DefaultTile>();
                    if(movementRange >= path.Count){
                        if (path.Count > 0){
                            movementRange = path.Count-1;
                        } else {
                            movementRange = 0;
                        }
                    }
                    actualPath.AddRange(path.GetRange(0, movementRange+1));

                    actualPath.First().Walkable = true;
                    actualPath.Last().Walkable = false;


                    movingCoroutine = EntityMoveSquares(actualPath, animator);
                    StartCoroutine(movingCoroutine);
                    currentTile.XPos = actualPath.Last().XPos;
                    currentTile.YPos = actualPath.Last().YPos;
                }
                else
                {
                    finishedMoving = true;
                }
            }
            else
            {
                finishedMoving = true;
            }
        }
        
        // Enumerator function for moving an entity along a path of tiles
        public IEnumerator EntityMoveSquares(List<DefaultTile> path, EntityAnimator animator = null)
        {
            GridLayout gr = WorldController.Instance.gridLayout;

            // Loop over each tile in the path (skipping the first one, since that's the entity's starting tile)
            for (int i = 1; i < path.Count; i++)
            {
                SoundManager.Instance.PlaySound(walkingClip);
                

                // Starts the walking animation of the entity
                if(animator)
                    animator.SetWalking();


                // Get the next tile in the path
                DefaultTile pathNode = path[i];
                
                // Calculate the direction to the target position and set the entity's rotation accordingly
                Vector3 targetPos = new Vector3(pathNode.GameObject.transform.position.x, transform.position.y, pathNode.GameObject.transform.position.z);
                Vector3 dir = (targetPos - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(dir,Vector3.up);
                targetLoc = PlayerMovement.SnapCoordinateToGrid(targetPos, gr);
                float time = 0; 
                
                // Loop until the entity has moved halfway to the target location
                while (time < 0.5f)
                {
                    // Adds the position and rotation
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time / 0.5f);
                    transform.position = Vector3.MoveTowards(transform.position, targetLoc, Time.deltaTime * 5);
                    time += Time.deltaTime;
                    yield return null;
                }
                transform.rotation = targetRotation;
            }
            // Turn off the walking sound

            finishedMoving = true;
        }

        public virtual void Update()
        {

            if (finishedMoving)
            {
                finishedMoving = false;
                Attack(attackingClip);
                StateController.ChangeState(GameState.EndTurn);
            }
        }
        
        public IEnumerator EnemyRotateToAttack()
        {
            Vector3 attackerPos = transform.position;
            Vector3 targetPos = WorldController.getPlayerTile().GameObject.transform.position;
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
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time / 0.5f);
                time += Time.deltaTime;
                yield return null;
            }
            transform.rotation = targetRotation;
        }
        
        static public GameObject getChildGameObject(GameObject fromGameObject, string withName) {
            Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
            foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
            
            return null;
        }
    }
}