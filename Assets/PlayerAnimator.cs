using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[System.Serializable]
public struct PlayerAnimator
{
    public enum Clip { Idle, Walk }
    const float transitionSpeed = 5f;

    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    Clip previousClip;
    float transitionProgress;

#if UNITY_EDITOR
    double clipTime;
#endif

    bool hasAppearClip, hasDisappearClip;
    public Clip CurrentClip { get; private set; }
    public bool IsDone => GetPlayable(CurrentClip).IsDone();

#if UNITY_EDITOR
    public bool IsValid => graph.IsValid();
#endif

    public void Configure(Animator animator, PlayerAnimatorConfig config)
    {
        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(
            graph, hasAppearClip || hasDisappearClip ? 6 : 4
        );

        var clip = AnimationClipPlayable.Create(graph, config.Idle);
        //clip.SetDuration(config.Idle.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Idle, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Walk);
        //clip.SetDuration(config.Walk.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Walk, clip, 0);

        var output = AnimationPlayableOutput.Create(graph, "Player", animator);
        output.SetSourcePlayable(mixer);
    }

#if UNITY_EDITOR
    public void RestoreAfterHotReload(
        Animator animator, PlayerAnimatorConfig config, float speed
    )
    {
        Configure(animator, config);
        GetPlayable(Clip.Walk).SetSpeed(speed);
        SetWeight(CurrentClip, 1f);
        var clip = GetPlayable(CurrentClip);
        clip.SetTime(clipTime);
        clip.Play();
        graph.Play();
        
    }
#endif

    public void GameUpdate()
    {
        if (transitionProgress >= 0f)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = -1f;
                SetWeight(CurrentClip, 1f);
                SetWeight(previousClip, 0f);
                GetPlayable(previousClip).Pause();
            }
            else
            {
                SetWeight(CurrentClip, transitionProgress);
                SetWeight(previousClip, 1f - transitionProgress);
            }
        }
#if UNITY_EDITOR
        clipTime = GetPlayable(CurrentClip).GetTime();
#endif
    }



    public void PlayWalk(float speed)
    {
        GetPlayable(Clip.Walk).SetSpeed(speed);
        BeginTransition(Clip.Walk);
    }

    public void PlayIdle(float speed)
    {
        GetPlayable(Clip.Idle).SetSpeed(speed);
        BeginTransition(Clip.Idle);
    }


    public void Stop()
    {
        graph.Stop();
    }

    public void Destroy()
    {
        graph.Destroy();
    }

    void BeginTransition(Clip nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = nextClip;
        transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }

    Playable GetPlayable(Clip clip)
    {
        return mixer.GetInput((int)clip);
    }

    void SetWeight(Clip clip, float weight)
    {
        mixer.SetInputWeight((int)clip, weight);
    }
}