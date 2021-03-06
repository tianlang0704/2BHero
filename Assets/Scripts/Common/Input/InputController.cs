﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum SwipeDirection {
    Left, Up, Right, Down, NA
}

public class InputController : MonoInjectable {
    private float shootPressBeginTime;
    private bool shootPressed = false;
    private float detectableAreaSize;

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

    public void VariableDurationShoot(
        Action<float, bool> end, 
        Action<float> inProgress = null, 
        Action start = null
    ) {
        // Keyboard shoot
        if (Input.GetKeyDown(KeyCode.F)) {
            BeginShootPress(start);
        }

        if (Input.GetKeyUp(KeyCode.F)) {
            EndShootPress(end, inProgress);
        }

        // Touch shoot
        if (Input.touchCount == 1 && Input.GetTouch(0).position.y < this.detectableAreaSize) {
            if (Input.GetTouch(0).phase == TouchPhase.Began) {
                BeginShootPress(start);
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended) {
                EndShootPress(end, inProgress);
            }
        }

        DuringShootPress(inProgress);
    }

    private void BeginShootPress(Action start) {
        this.shootPressBeginTime = Time.time;
        this.shootPressed = true;
        if (start != null) { start(); }
    }

    private void DuringShootPress(Action<float> inProgress) {
        if (shootPressed) {
            if (inProgress != null) { inProgress(Time.time - this.shootPressBeginTime); }
        }
    }

    private void EndShootPress(Action<float, bool> end, Action<float> inProgress = null) {
        this.shootPressed = false;
        if (inProgress != null) { inProgress(0f); }
        float timePassed = Time.time - this.shootPressBeginTime;
        //Set success flag to be false when it ends up to be a swipe
        if (!this.isSwipe) {
            end(timePassed, true);
        }else {
            end(timePassed, false);
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
                        gestureSqrDist > this.minSqrSwipeDist
                    ) {
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
    override protected void Awake() {
        base.Awake();
        this.minSqrSwipeDist = this.minSwipeDist * this.minSwipeDist;
        this.detectableAreaSize = Screen.height * 0.9f;
    }
// End: Singleton initialization
}
