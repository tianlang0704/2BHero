using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour {
    [Header("Sound")]
    [SerializeField] private List<AudioClip> _moveSounds = new RandomList<AudioClip>();
    public RandomList<AudioClip> moveSounds { get { return (RandomList<AudioClip>)_moveSounds; } }

    private AudioSource audioSource;

    public void Move(GameObject mark) {
        this.transform.position = mark.transform.position;
        this.audioSource.PlayOneShot(this.moveSounds.GetRandom());
    }

    protected virtual void Awake() {
        this.audioSource = this.GetComponent<AudioSource>();
    }
}
