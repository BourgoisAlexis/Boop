using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour {
    [SerializeField] private List<AudioClip> _clips = new List<AudioClip>();

    private List<AudioSource> _sources;
    private int _sourceIndex = 0;
    private int _sourceNumber = 5;
    private int _maxSourceNumber = 10;


    public void Init() {
        _sources = new List<AudioSource>();

        for (int i = 0; i < _sourceNumber; i++)
            CreateSource();
    }

    private AudioSource CreateSource() {
        AudioSource source = Camera.main.gameObject.AddComponent<AudioSource>();
        _sources.Add(source);
        return source;
    }

    public void PlayAudio(int index, float volume = 1) {
        AudioClip clip = _clips[index];

        if (clip == null)
            return;

        _sourceIndex++;

        if (_sourceIndex >= _sources.Count)
            _sourceIndex = 0;

        AudioSource source = _sources[_sourceIndex];

        if (source.isPlaying) {
            bool found = false;

            foreach (AudioSource s in _sources) {
                if (!s.isPlaying) {
                    source = s;
                    found = true;
                    break;
                }
            }

            if (!found) {
                if (_sources.Count >= _maxSourceNumber)
                    return;
                else
                    source = CreateSource();
            }
        }

        source.pitch = Random.Range(0.9f, 1.1f);
        source.volume = volume;
        source.clip = clip;
        source.Play();
    }
}
