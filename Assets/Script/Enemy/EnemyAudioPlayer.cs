using UnityEngine;

namespace Game.Enemy
{
    public class EnemyAudioPlayer : MonoBehaviour
    {

        public Sound footStep;
        public Sound moanA;
        public Sound moanB;
        public Sound moanC;
        public Sound moanD;
        public Sound moanE;
        public Sound moanF;

        Sound[] playlist;
        int playlistSize = 7;
        Animator animator;
        AnimationClip[] animationClips;
        AnimationEvent playRandomEvent;

        private void Awake()
        {
            playlist = new Sound[playlistSize];
            playlist[0] = footStep;
            playlist[1] = moanA;
            playlist[2] = moanB;
            playlist[3] = moanC;
            playlist[4] = moanD;
            playlist[5] = moanE;
            playlist[6] = moanF;

            foreach (Sound sound in playlist)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                Sound.SoundtoSource(source, sound);
            }

            playRandomEvent = new AnimationEvent();
            playRandomEvent.functionName = "PlayRandomMoan";
            playRandomEvent.time = 0;

            animator = GetComponent<Animator>();
            animationClips = animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in animationClips)
                clip.AddEvent(playRandomEvent);

        }

        public void PlayFootStep()
        {
            PlayAudio(footStep);
        }

        public void PlayRandomMoan()
        {
            int index = Random.Range(1, 7);

            switch (index)
            {
                case 1:
                    moanA.Play();
                    return;
                case 2:
                    moanB.Play();
                    return;
                case 3:
                    moanC.Play();
                    return;
                case 4:
                    moanD.Play();
                    return;
                case 5:
                    moanE.Play();
                    return;
                case 6:
                    moanF.Play();
                    return;
                default:
                    moanA.Play();
                    return;
            }
        }

        void PlayAudio(Sound sound)
        {
            sound.Play();
        }

        void PauseAudio(Sound sound)
        {
            sound.Pause();
        }

        void StopAudio(Sound sound)
        {
            sound.Stop();
        }

    }

}
