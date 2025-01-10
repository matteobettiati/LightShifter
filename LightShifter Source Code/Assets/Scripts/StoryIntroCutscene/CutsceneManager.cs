using System;
using System.Collections;
using System.Collections.Generic;
using LightShift;
using TarodevController;
using TMPro;
using UnityEngine;
using Newtonsoft.Json;

namespace StoryIntroCutscene {
public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private LightShifter lightShifter;
    [SerializeField] private TextMeshProUGUI text;
    private FrameInput _cutsceneframeInput;
    private bool _enableParallax, _freezeMovement;
    private int _currentStoryTextIndex = 0;
    private List<StoryText> _storyTexts;
    void Start()
    {
        var jsonStoryTexts = Resources.Load<TextAsset>("StoryTexts");
        _storyTexts = JsonConvert.DeserializeObject<List<StoryText>>(jsonStoryTexts.text);

        _cutsceneframeInput = new FrameInput
        {
            JumpDown = false,
            JumpHeld = false,
            DashDown = false,
            Move = new Vector2(0,0),
            LightShift = false
        };
        
        StartCoroutine(CutsceneCoroutine());
        StartCoroutine(StoryTextCouroutine());
    }

    public FrameInput GetFrameInput() {
        return _cutsceneframeInput;
    }

    public bool GetEnabledParallax() {
        return _enableParallax;
    }
    public bool GetFreezePlayerMovement() {
        return _freezeMovement;
    }

    private IEnumerator StoryTextCouroutine() {
        _currentStoryTextIndex = 0;
        foreach (StoryText x in _storyTexts) {
            text.text = x.Text;
            text.CrossFadeAlpha(1, 1.5f, false);
            yield return new WaitForSeconds(1.5f);
            yield return new WaitForSeconds((float) x.Duration);
            text.CrossFadeAlpha(0, 1.5f, false);
            yield return new WaitForSeconds(1.5f);
        }

        yield return null;
    }

    private void GoToNextStoryText() {
        
    }

    private IEnumerator CutsceneCoroutine() {
        _enableParallax = false;
        _freezeMovement = false;
        // wait 1.5 seconds
        yield return new WaitForSeconds(1.5f);
        // block the shift after waiting, to ensure the LightShifter component has been loaded previously
        lightShifter.BlockShift();

        // move to the right until reached x = -29
        _cutsceneframeInput.Move = new Vector2(1,0);
        while(player.transform.position.x <= -29.35)
            yield return 0;
        // stop moving and wait 1 second
        _cutsceneframeInput.Move = new Vector2(0,0);
        yield return new WaitForSeconds(1f);

        // flip the player, wait 1 second, flip again and wait 1 second
        _freezeMovement = true;
        _cutsceneframeInput.Move = new Vector2(-0.001f, 0);
        yield return new WaitForSeconds(1f);
        _cutsceneframeInput.Move = new Vector2(+0.001f, 0);
        yield return new WaitForSeconds(1f);

        // start running on the spot towards the right
        _enableParallax = true;
        _cutsceneframeInput.Move = new Vector2(1, 0);

        yield return new WaitForSeconds(3f);
        lightShifter.ToggleEnvironment();

        yield return new WaitForSeconds(3f);
        lightShifter.ToggleEnvironment();

        yield return null;
    }
    private static readonly int CutsceneKey = Animator.StringToHash("Cutscene");
    
}

    [System.Serializable]
    class StoryText {
        public double Index { get; set; }
        public string Text { get; set; }
        public double Duration { get; set; }
    }

}

