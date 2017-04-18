using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogPause : PopupDialogBase {
    public int countdownSeconds = 3;
    public string countdownTextPrefix = "Resume In ";
    public string pauseTextString = "Paused";
    public Text pauseText;

    private int countdownRemining;
    private bool isCountingDown = false;
    private Coroutine countDownRoutine = null;

    public override void Show(Action closeAction = null) {
        this.pauseText.text = this.pauseTextString;
        base.Show(closeAction);
    }

    public override void CloseMenu() {
        if (this.isCountingDown) { return; }
        this.countDownRoutine = StartCoroutine(CountDownRoutine(() => {
            this.countDownRoutine = null;
            base.CloseMenu();
        }));
    }

    public void CancelCountdown() {
        if(this.countDownRoutine == null) { return; }
        this.isCountingDown = false;
        this.pauseText.text = this.pauseTextString;
        StopCoroutine(this.countDownRoutine);
    }

    private IEnumerator CountDownRoutine(Action end) {
        this.isCountingDown = true;
        this.countdownRemining = this.countdownSeconds;
        while (this.countdownRemining > 0) {
            this.pauseText.text = this.countdownTextPrefix + this.countdownRemining.ToString();
            yield return new WaitForSecondsRealtime(1f);
            this.countdownRemining = this.countdownRemining - 1 > 0 ? this.countdownRemining - 1 : 0;
        }
        this.isCountingDown = false;
        end();
    }
}
