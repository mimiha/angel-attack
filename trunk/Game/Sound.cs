using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace guiCreator
{
    public class SoundMgr
    {
        SoundEffectInstance sndCtrl;
        string directory,   //directory of sound
            name,           //which character is the sound prefixed with?
            type;        //suffix of .wav file
        int max;            //max soundfiles to random (last soundfile # + 1) 

        public void PlayVoice(ContentManager theContentManager, int actor, int snd)
        {
            directory = "audio//voice//";

            //max of -1 means it only has one of its type and needs no random.
            switch (snd)
            {
                case 1: type = "_attack";   max = 5; break;
                case 2: type = "_hurt";     max = 5; break;
                case 3: type = "_death";    max = 4; break;
                case 4: type = "_special";  max = 4; break;
                default: type = null; max = -1; break;
            }

            switch (actor)
            {
                case 1: name = "architect" + type; break;
                case 2: name = "noble" + type; break;
                case 3: name = "grenadier" + type; break;
                case 4: name = "espion" + type; break;
                case 5: name = "lancer" + type; break;
                default: name = "null"; break;
            }

            SoundEffect soundFX;

            //random number
            if (max != -1)
            {
                Random rndsnd = new Random();
                int rand = rndsnd.Next(1, max);
                soundFX = theContentManager.Load<SoundEffect>(directory + name + rand);
            }
            else
                soundFX = theContentManager.Load<SoundEffect>(directory + name);

            sndCtrl = soundFX.Play();

            if (sndCtrl != null)
            {
                if (sndCtrl.State == SoundState.Stopped)
                {
                    sndCtrl.Dispose();
                    sndCtrl = null;
                }

            }

        }   //end voice function



        public void PlayEffect(ContentManager theContentManager, int effect)
        {
            directory = "audio//effect//";

            //switches effect names. 
            //max of -1 means it only has one of its type and needs no random.
            switch (effect)
            {
                case 1: name = "walk_concrete"; max = 5; break;
                case 2: name = "knife_swing"; max = 4; break;
                case 3: name = "hitconnect"; max = 4; break;
                default: name = "null"; max = -1; break;
            }
            SoundEffect soundFX;

            //random number
            if (max != -1)
            {
                Random rndsnd = new Random();
                int rand = rndsnd.Next(1, max);
                soundFX = theContentManager.Load<SoundEffect>(directory + name + rand);
            }
            else
                soundFX = theContentManager.Load<SoundEffect>(directory + name);


                
            sndCtrl = soundFX.Play();


            if (sndCtrl != null)
            {
                if (sndCtrl.State == SoundState.Stopped)
                {
                    sndCtrl.Dispose();
                    sndCtrl = null;
                }

            }

        }   //end effect function


    }
}
