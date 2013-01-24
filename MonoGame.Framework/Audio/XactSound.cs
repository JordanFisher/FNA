using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
	internal class XactSound
	{
        internal uint category;
        
		bool complexSound;
		XactClip[] soundClips;
		SoundEffectInstance wave;
		
		public XactSound (SoundBank soundBank, BinaryReader soundReader, uint soundOffset)
		{
			long oldPosition = soundReader.BaseStream.Position;
			soundReader.BaseStream.Seek (soundOffset, SeekOrigin.Begin);
			
			byte flags = soundReader.ReadByte ();
			complexSound = (flags & 1) != 0;
			
			category = soundReader.ReadUInt16 ();
			uint volume = soundReader.ReadByte (); // FIXME: Maybe wrong?
			uint pitch = soundReader.ReadUInt16 (); // FIXME: Maybe wrong?
			soundReader.ReadByte (); //unkn
			uint entryLength = soundReader.ReadUInt16 ();
			
			uint numClips = 0;
			if (complexSound) {
				numClips = (uint)soundReader.ReadByte ();
			} else {
				uint trackIndex = soundReader.ReadUInt16 ();
				byte waveBankIndex = soundReader.ReadByte ();
				wave = soundBank.GetWave(waveBankIndex, trackIndex);
			}
			
			if ( (flags & 0x1E) != 0 ) {
				uint extraDataLen = soundReader.ReadUInt16 ();
				//TODO: Parse RPC+DSP stuff
				
				soundReader.BaseStream.Seek (extraDataLen - 2, SeekOrigin.Current);
			}
			
			if (complexSound) {
				soundClips = new XactClip[numClips];
				for (int i=0; i<numClips; i++) {
					soundReader.ReadByte (); //unkn
					uint clipOffset = soundReader.ReadUInt32 ();
					soundReader.ReadUInt32 (); //unkn
					
					soundClips[i] = new XactClip(soundBank, soundReader, clipOffset);
				}
			}
            
            // FIXME: This is totally arbitrary. I dunno the exact ratio here.
            Volume = volume / 256.0f;
			
			soundReader.BaseStream.Seek (oldPosition, SeekOrigin.Begin);
		}
		
//		public XactSound (Sound sound) {
//			complexSound = false;
//			wave = sound;
//		}
		public XactSound (SoundEffectInstance sound) {
			complexSound = false;
			wave = sound;
		}		
        
		public void Play() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Play();
				}
			} else {
				if (wave.State == SoundState.Playing) wave.Stop ();
				wave.Play ();
			}
		}
        
        internal void PlayPositional(AudioListener listener, AudioEmitter emitter) {
            if (complexSound) {
                foreach (XactClip clip in soundClips) {
                    clip.PlayPositional(listener, emitter);
                }
            } else {
                if (wave.State == SoundState.Playing) wave.Stop();
                wave.Apply3D(listener, emitter);
                wave.Play();
            }
        }
        
        internal void UpdatePosition(AudioListener listener, AudioEmitter emitter)
        {
            if (complexSound) {
                foreach (XactClip clip in soundClips) {
                    clip.UpdatePosition(listener, emitter);
                }
            } else {
                wave.Apply3D(listener, emitter);
            }
            
        }
		
		public void Stop() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Stop();
				}
			} else {
				wave.Stop ();
			}
		}
		
		public void Pause() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Pause();
				}
			} else {
				wave.Pause ();
			}
		}
                
		public void Resume() {
			if (complexSound) {
				foreach (XactClip clip in soundClips) {
					clip.Play();
				}
			} else {
				wave.Resume ();
			}
		}
		
		public float Volume {
			get {
				if (complexSound) {
					return soundClips[0].Volume;
				} else {
					return wave.Volume;
				}
			}
			set {
				if (complexSound) {
					foreach (XactClip clip in soundClips) {
						clip.Volume = value;
					}
				} else {
					wave.Volume = value;
				}
			}
		}
		
		public bool Playing {
			get {
				if (complexSound) {
					foreach (XactClip clip in soundClips) {
						if (clip.Playing) return true;
					}
					return false;
				} else {
					return wave.State == SoundState.Playing;
				}
			}
		}
		
	}
}

