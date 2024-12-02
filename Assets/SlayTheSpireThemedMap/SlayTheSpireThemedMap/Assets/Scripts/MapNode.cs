using System;
using DG.Tweening;
using NueGames.NueDeck.Scripts.Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Map
{
    public enum NodeStates
    {
        Locked,
        Visited,
        Attainable
    }
}

namespace Map
{
    public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Transform spotlightRadius;
        public float spotlightOuterRadius = 32f;

        public Light2D spotlight;
        public SpriteRenderer sr;
        public Image image;
        public SpriteRenderer visitedCircle;
        public Image circleImage;
        public Image visitedCircleImage;

        public Node Node { get; private set; }
        public NodeBlueprint Blueprint { get; private set; }

        private float initialScale;
        private const float HoverScaleFactor = 1.2f;
        private float mouseDownTime;

        private const float MaxClickDuration = 0.5f;

        void Update()
        {
            
            // Vector2 Mouseposistion = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // if(IsPointerinSpotLightRange(Mouseposistion))
            //     {
            //         Debug.Log("mouse within light, currently in:" + Mouseposistion);
            //     }
            //     else
            //     {
            //        Debug.Log("mouse outside light, currently in:" + Mouseposistion);
            //     }

           
            
        }

        public void SetUp(Node node, NodeBlueprint blueprint)
        {
            Node = node;
            Blueprint = blueprint;
            if (spotlight != null)
            {
                spotlight.enabled = false; // Turn off by default
                //spotlight.color = MapView.Instance.visitedColor; // Example color configuration
            }
            if (sr != null) sr.sprite = blueprint.sprite;
            if (image != null) image.sprite = blueprint.sprite;
            if (node.nodeType == NodeType.Boss) transform.localScale *= 1.5f;
            if (sr != null) initialScale = sr.transform.localScale.x;
            if (image != null) initialScale = image.transform.localScale.x;

            if (visitedCircle != null)
            {
                visitedCircle.color = MapView.Instance.visitedColor;
                visitedCircle.gameObject.SetActive(false);
            }

            if (circleImage != null)
            {
                circleImage.color = MapView.Instance.visitedColor;
                circleImage.gameObject.SetActive(false);    
            }
            
            SetState(NodeStates.Locked);
        }

        public void HighlightNode(bool highlight)
        {
            if (spotlight != null)
            {
                spotlight.enabled = highlight;
            }
        }

        bool IsPointerinSpotLightRange(Vector2 pointerPosition)
        {
            Vector2 spotlightPosition = spotlight.transform.position;
            float outerRadius = spotlight.pointLightOuterRadius;
            float distance = Vector2.Distance(pointerPosition, spotlightPosition);
            
            return distance <= outerRadius;
        }

        public void LightState(int value)
        {
            switch(value)
            {
                case >= 90 and <= 100:
                spotlightOuterRadius = 32f;
                spotlight.pointLightInnerRadius = 16f; 
                spotlight.pointLightOuterRadius = 32f;
                break;
                case >= 50 and <= 89:
                spotlightOuterRadius = 20f;
                spotlight.pointLightInnerRadius = 10f; 
                spotlight.pointLightOuterRadius = 20f;
                break;
                case >= 10 and <= 49:
                spotlightOuterRadius = 10f;
                spotlight.pointLightInnerRadius = 5f; 
                spotlight.pointLightOuterRadius = 10f;
                break;
                case >= 1 and <= 9:
                spotlightOuterRadius = 9f;
                spotlight.pointLightInnerRadius = 2f; 
                spotlight.pointLightOuterRadius = 4f;
                break;
                case 0:
                spotlightOuterRadius = 1.3f;
                spotlight.pointLightInnerRadius = 0.6f; 
                spotlight.pointLightOuterRadius = 1.3f;
                break;

            }

        }

        public void SetState(NodeStates state)
        {
            if (visitedCircle != null) visitedCircle.gameObject.SetActive(false);
            if (circleImage != null) circleImage.gameObject.SetActive(false);
            
            switch (state)
            {
                case NodeStates.Locked:
                    LightState(GameManager.Instance.PersistentGameplayData.light);
                     if(spotlight != null)
                    {
                        spotlight.enabled = false;
                    }
                    if (sr != null)
                    {
                        sr.DOKill();
                        sr.color = MapView.Instance.lockedColor;
                    }

                    if (image != null)
                    {
                        image.DOKill();
                        image.color = MapView.Instance.lockedColor;
                    }

                    break;
                case NodeStates.Visited:
                    LightState(GameManager.Instance.PersistentGameplayData.light);
                    if(spotlight != null)
                    {
                        spotlight.enabled = false;
                    }

                    if (sr != null)
                    {
                        sr.DOKill();
                        sr.color = MapView.Instance.visitedColor;
                    }
                    
                    if (image != null)
                    {
                        image.DOKill();
                        image.color = MapView.Instance.visitedColor;
                    }
                    
                    if (visitedCircle != null) visitedCircle.gameObject.SetActive(true);
                    if (circleImage != null) circleImage.gameObject.SetActive(true);
                    break;
                case NodeStates.Attainable:
                    // start pulsating from visited to locked color:
                    LightState(GameManager.Instance.PersistentGameplayData.light);
                    if(spotlight != null)
                    {
                        spotlight.enabled = true;
                    }
                    if (sr != null)
                    {
                        sr.color = MapView.Instance.lockedColor;
                        sr.DOKill();
                        sr.DOColor(MapView.Instance.visitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }
                    
                    if (image != null)
                    {
                        image.color = MapView.Instance.lockedColor;
                        image.DOKill();
                        image.DOColor(MapView.Instance.visitedColor, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void EncounterDetails()
        {
            // Vector2 Mouseposistion = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // if(IsPointerinSpotLightRange(Mouseposistion))
            //     {
            //         MapView.Instance.NodeDetails.SetActive(true);
            //     }
            //     else
            //     {
            //         MapView.Instance.NodeDetails.SetActive(false);
            //     }
            MapView.Instance.NodeDetails.SetActive(true);

            switch(Node.nodeType)
            {
                case NodeType.MinorEnemy:
                MapView.Instance.NodeDescription.text = "Those who stand in your way—some fight for survival, while some simply do it for the thrill of it, all the cruel reasons that exist. It's up to you whether to face them or not.";
                MapView.Instance.NodeTitle.text = "Battle";
                break;

                case NodeType.EliteEnemy:
                MapView.Instance.NodeDescription.text = "Tested by the harsh reality of this world, they have molded themselves into something stronger. Tread carefully, for in their eyes, you're nothing more than an insignificant obstacle in their way.";
                MapView.Instance.NodeTitle.text = "Mighty Foe";
                break;

                case NodeType.Store:
                MapView.Instance.NodeDescription.text = "Neither a foe nor an ally, some people like to stay in the sidelines. Say what you want about him, but right now, he's the closest thing you got as a friend.";
                MapView.Instance.NodeTitle.text = "Merchant";
                break;

                case NodeType.RestSite:
                MapView.Instance.NodeDescription.text = "Though temporary, it's a safespace to shelter yourself from the dangers of the world. Make good use of it.";
                MapView.Instance.NodeTitle.text = "Respite";
                break;

                case NodeType.Mystery:
                MapView.Instance.NodeDescription.text = "The world is filled with surprises. Experience it with modesty.";
                MapView.Instance.NodeTitle.text = "Encounter";
                break;

                case NodeType.Boss:
                MapView.Instance.NodeDescription.text = "A heavy presence that blocks the path ahead. A relentless force, unyielding and merciless. They won't stand down. But neither will you.";
                MapView.Instance.NodeTitle.text = "Dreadful Foe";
                break;

            }
            
        }

        public void OnPointerEnter(PointerEventData data)
        {
           // HighlightNode(true);
            EncounterDetails();
            if (sr != null)
            {
                sr.transform.DOKill();
                sr.transform.DOScale(initialScale * HoverScaleFactor, 0.3f);
            }

            if (image != null)
            {
                image.transform.DOKill();
                image.transform.DOScale(initialScale * HoverScaleFactor, 0.3f);
            }
        }

        public void OnPointerExit(PointerEventData data)
        {
           // HighlightNode(false);
            MapView.Instance.NodeDetails.SetActive(false);
            if (sr != null)
            {
                sr.transform.DOKill();
                sr.transform.DOScale(initialScale, 0.3f);
            }

            if (image != null)
            {
                image.transform.DOKill();
                image.transform.DOScale(initialScale, 0.3f);
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            mouseDownTime = Time.time;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (Time.time - mouseDownTime < MaxClickDuration)
            {
                // user clicked on this node:
                MapPlayerTracker.Instance.SelectNode(this);
            }
        }

        public void ShowSwirlAnimation()
        {
            if (visitedCircleImage == null)
                return;

            const float fillDuration = 0.3f;
            visitedCircleImage.fillAmount = 0;

            DOTween.To(() => visitedCircleImage.fillAmount, x => visitedCircleImage.fillAmount = x, 1f, fillDuration);
        }

        private void OnDestroy()
        {
            if (image != null)
            {
                image.transform.DOKill();
                image.DOKill();
            }
            if (sr != null)
            {
                sr.transform.DOKill();
                sr.DOKill();
            }
        }
    }
}
