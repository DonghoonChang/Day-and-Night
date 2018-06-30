using UnityEngine;

namespace MyGame.Player
{
    public class PlayerAudioPlayer : MonoBehaviour
    {

        public Sound[] hitSounds;
        public Sound[] deathSounds;
        public Sound[] footstepSounds;

        int footstepIndex = 0;

        private void Awake()
        {
            if (hitSounds.Length != 0)
                foreach (Sound sound in hitSounds)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    Sound.SoundtoSource(source, sound);
                }

            if (deathSounds.Length != 0)
                foreach (Sound sound in deathSounds)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    Sound.SoundtoSource(source, sound);
                }

            if (footstepSounds.Length != 0)
                foreach (Sound sound in footstepSounds)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    Sound.SoundtoSource(source, sound);
                }
        }

        public int FootstepIndex
        {
            set
            {
                footstepIndex = value % 6;
            }
        }


        void PlayFootStep(AnimationEvent evt)
        {
            if (evt.animatorClipInfo.weight > 0.80f)
            {
                footstepSounds[footstepIndex].Play();
            }
        }

        void PlayHitSound()
        {

        }

        void PlayDeathSound()
        {

        }

    }

}
