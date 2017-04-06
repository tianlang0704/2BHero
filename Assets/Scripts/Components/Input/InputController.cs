using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum SwipeDirection {
    Left, Up, Right, Down, NA
}

public class InputController : ControllerBase {
    private float shootPressBeginTime;

    public void PlayerMoveUp(Action action) {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            action();
        }

        if(SwipeDetection() == SwipeDirection.Up) {
            action();
        }
    }

    public void PlayerMoveDown(Action action) {
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            action();
        }

        if (SwipeDetection() == SwipeDirection.Down) {
            action();
        }
    }

    public void VariableDurationShoot(Action<float> action) {
        if (Input.GetKeyDown(KeyCode.F)) {
            this.shootPressBeginTime = Time.time;
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            float timePassed = Time.time - this.shootPressBeginTime;
            action(timePassed);
        }

        if (Input.touchCount == 1) {
            if (Input.GetTouch(0).phase == TouchPhase.Began) {
                this.shootPressBeginTime = Time.time;
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended && !this.isSwipe) {
                float timePassed = Time.time - this.shootPressBeginTime;
                action(timePassed);
            }
        }
    }
// Mark: Swipe functions
    public float minSwipeDist = 50.0f;
    public float maxSwipeTime = 0.5f;

    private float minSqrSwipeDist = 0.0f;
    private float fingerStartTime = 0.0f;
    private Vector2 fingerStartPos = Vector2.zero;
    private bool isSwipe = false;

    private SwipeDirection SwipeDetection() {
        if (Input.touchCount > 0) {
            Touch touch = Input.touches[0];
            switch (touch.phase) {
                case TouchPhase.Began:
                    /* this is a new touch */
                    this.isSwipe = false;
                    this.fingerStartTime = Time.time;
                    this.fingerStartPos = touch.position;
                    break;
                case TouchPhase.Moved:
                    if((touch.position - this.fingerStartPos).sqrMagnitude > this.minSqrSwipeDist) {
                        this.isSwipe = true;
                    }
                    break;
                case TouchPhase.Canceled:
                    /* The touch is being canceled */
                    this.isSwipe = false;
                    Debug.Log("Swipe Cancelled");
                    break;
                case TouchPhase.Ended:
                    float gestureTime = Time.time - fingerStartTime;
                    float gestureSqrDist = (touch.position - this.fingerStartPos).sqrMagnitude;

                    if (this.isSwipe && 
                        gestureTime < this.maxSwipeTime && 
                        gestureSqrDist > this.minSqrSwipeDist) {
                        Vector2 direction = touch.position - this.fingerStartPos;
                        Vector2 swipeType = Vector2.zero;

                        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
                            // the swipe is horizontal:
                            swipeType = Vector2.right * Mathf.Sign(direction.x);
                        } else {
                            // the swipe is vertical:
                            swipeType = Vector2.up * Mathf.Sign(direction.y);
                        }

                        if (swipeType.x != 0.0f) {
                            if (swipeType.x > 0.0f) {
                                return SwipeDirection.Right;
                            } else {
                                return SwipeDirection.Left;
                            }
                        }

                        if (swipeType.y != 0.0f) {
                            if (swipeType.y > 0.0f) {
                                return SwipeDirection.Up;
                            } else {
                                return SwipeDirection.Down;
                            }
                        }
                    }
                    break;
            }
        }
        return SwipeDirection.NA;
    }
// End: Swipe functions

// Mark: Singleton initialization
    public static InputController shared = null;
    override protected void Awake() {
        if (InputController.shared == null) {
            InputController.shared = this;
        } else if (InputController.shared != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        this.minSqrSwipeDist = this.minSwipeDist * this.minSwipeDist;
    }
// End: Singleton initialization
}
