using System;
using Audiox.Editor.Managers;
using Audiox.Editor.Models;
using Audiox.Runtime.Assets;
using Audiox.Runtime.Models;
using Audiox.Runtime.Tools;
using UnityEditor;
using UnityEngine;
using Input = Audiox.Editor.Models.Input;

namespace Audiox.Editor
{
    public class AudioxEditorWindow : EditorWindow
    {
        public static AudioxEditorWindow Instance;
        
        private const double DoubleClickTime = 0.3;
        private const float SeparatorHeight = 6f;
        private const float MinCurveHeight = 100f;
        public const float LabelWidth = 75f;
        public const float ScrollHeight = 32f;
        public const string Title = "Audiox Samples Editor";
        
        [SerializeField] private AudioxAudioManager audioManager;
        [SerializeField] private AudioxSampleLibraryAsset loadedAsset;
        [SerializeField] private string samplesAssetPath = string.Empty;
        
        public AudioxSampleLibraryAsset Data;
        
        private Input _input;
        private Curve _curve;
        private Timeline _timeline;
        private Samples _samples;

        private Sample _audioPlayingSample;
        private bool _isPlayingSample;
        private bool _isPlaying;
        private bool _isDataLoaded;

        private Rect _curvePanel;
        private Rect _curveRect;
        private Rect _doubleClickRect;
        private Rect _textAreaRect;
        private Rect _resizeRect;
        private Vector2 _curvePadding = new Vector2(30f, 30);
        private Vector2 _scrollTextAreaPosition;
        private Vector2 _scrollPosition;
        private bool _isResize;
        private bool _autoSave;
        private double _clickTime;
        private float _curveSize;
        private float _curveHeight;
        
        private readonly Color _audioCursorColor = new Color(203 / 255f, 88 / 255f, 88 / 255f, 255 / 255f);

        private bool AutoSave
        {
            get => _autoSave && samplesAssetPath != string.Empty;
            set
            {
                _autoSave = value;
                EditorPrefs.SetBool("ATS_AutoSave", _autoSave);
            }
        }

        [MenuItem("Window/Audiox Samples Editor", false, 111)]
        private static void OpenWindow()
        {
            Instance = GetWindow<AudioxEditorWindow>(Title, true);
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnMouseDrag(int controlId)
        {
            if (GUIUtility.hotControl == controlId)
            {
                if (Data != null)
                {
                    Undo.RecordObject(Data, "Moved Samples");
                }
                _samples.OnDragSample(_curveRect.width, _curvePadding.x);
                Event.current.Use();
            }
        }

        private void OnMouseWheel()
        {
            if (Mathf.Abs(Event.current.delta.y) > 2f && !_textAreaRect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = 0;
                Event.current.Use();
                _curveSize -= Event.current.delta.y / 100f;
                if (_curveSize < 1f)
                {
                    _curveSize = 1f;
                }
            }
        }

        private void OnMouseDown(int controlId)
        {
            if (_curvePanel.Contains(Event.current.mousePosition))
            {
                _samples.OnMouseDown(_curvePanel.height);
                GUIUtility.hotControl = controlId;
                Event.current.Use();
            }
        }

        private void OnDestroy()
        {
            audioManager.Destroy();
        }

        private void OnGUI()
        {
            DrawTopPanel();
            
            if (Data.Clip != null)
            {
                CalculateRects();
                
                EditorGUILayout.BeginHorizontal();
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(_curveHeight));
                _input.Update();
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                _timeline.Draw(Data.Clip.length);
                EditorGUILayout.EndHorizontal();
                GUILayoutUtility.GetRect(_curveRect.width + _curvePadding.x * 2f, _curveHeight - _curvePadding.y);
                _curve.Render();
                _samples.Draw();
                OnDoubleClick();
                DrawAudioCursor();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
                
                DrawResizeRect();
            }
            
            DrawBottomPanel();
            DrawSeparator();
            DrawAudioClipField();
            
            _samples.DrawInfo();
            
            DrawTextField();
        }

        private void Update()
        {
            if (audioManager.Source != null && audioManager.Source.isPlaying)
            {
                if (_isPlayingSample && _audioPlayingSample != null && audioManager.Source.time >= _audioPlayingSample.EndPosition)
                {
                    audioManager.Source.Pause();
                    _isPlayingSample = false;
                }
                Repaint();
            }
        }

        private void CalculateRects()
        {
            if (Mathf.Abs(_curveHeight) < 1f)
            {
                _curveHeight = position.height * 0.4f;
            }

            _curveHeight = Mathf.Clamp(_curveHeight, MinCurveHeight, position.height * 0.5f);
            _curvePadding = new Vector2(30f, EditorStyles.toolbar.fixedHeight);
            _curvePanel = new Rect(_curvePadding.x, _curvePadding.y, position.width * _curveSize, _curveHeight);
            _curveRect = new Rect(_curvePadding.x, _curvePadding.y, position.width * _curveSize - _curvePadding.x * 2f, _curveHeight - _curvePadding.y * 2f);
            _doubleClickRect = new Rect(0, _curvePadding.y, position.width * _curveSize, _curveHeight);
            _resizeRect = new Rect(0, _curvePanel.height + _curvePadding.y + EditorStyles.toolbarButton.fixedHeight + 3f, position.width, SeparatorHeight);
        }

        private void DrawResizeRect()
        {
            if (Event.current.type == EventType.MouseDown && _resizeRect.Contains(Event.current.mousePosition))
            {
                _isResize = true;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                _isResize = false;
            }
            if (_isResize)
            {
                _curveHeight = Event.current.mousePosition.y - _curvePadding.y * 1.5f + SeparatorHeight / 2f;
                _curveHeight = Mathf.Clamp(_curveHeight, MinCurveHeight, position.height);
                _resizeRect = new Rect(0, _curveRect.height + _curvePadding.y * 3f + 9f, position.width, SeparatorHeight);
                Repaint();
            }
            EditorGUIUtility.AddCursorRect(_resizeRect, MouseCursor.ResizeVertical);
        }

        private void OnDoubleClick()
        {
            if (Event.current.button == 0 && Event.current.clickCount > 0 && _doubleClickRect.Contains(Event.current.mousePosition))
            {
                if (EditorApplication.timeSinceStartup - _clickTime < DoubleClickTime && Event.current.clickCount % 2 == 0)
                {
                    var time = ((Event.current.mousePosition.x - _curvePadding.x) / _curveRect.width) * Data.Clip.length;
                    time = Mathf.Clamp(time, 0f, Data.Clip.length - 0.0001f);
                    if (!audioManager.IsStartPlaying())
                    {
                        audioManager.Source.Play();
                    }
                    audioManager.Source.Pause();
                    audioManager.SetPosition(time);
                    Event.current.clickCount = 0;
                }
                _clickTime = EditorApplication.timeSinceStartup;
            }
        }

        private void DrawTopPanel()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent("New", "Create a new samples"), EditorStyles.toolbarButton) && Data != null && Data.Samples.Count > 0 &&
                EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to create new samples? All your current changes will be lost.", "Yes", "Cancel"))
            {
                CreateAsset();
                _samples.Deselect();
            }

            if (GUILayout.Button(new GUIContent("Load", "Load existing asset with samples"), EditorStyles.toolbarButton))
            {
                var path = EditorUtility.OpenFilePanel("Title", "Directory", "asset");
                LoadAsset(path);
            }

            if ((!string.IsNullOrEmpty(samplesAssetPath) || Data.Clip != null) && GUILayout.Button(new GUIContent("Save", "Save current samples to asset"), EditorStyles.toolbarButton))
            {
                SaveAsset();
            }

            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(samplesAssetPath == string.Empty);
            
            AutoSave = GUILayout.Toggle(_autoSave, "Auto Save", EditorStyles.toggle);
            
            EditorGUI.EndDisabledGroup();
            
            if (EditorGUI.EndChangeCheck() && (!AutoSave || AutoSave && EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to switch to Auto Save mode? All your unsaved changes will be lost.", "Yes", "Cancel")))
            {
                UpdateDataAsset();
                Init(true);
            }
            
            GUILayout.Label("| ", EditorStyles.label);
            _curveSize = Mathf.Clamp(_curveSize, 1f, _curveSize);
            var zoomValue = _curveSize.ToString("0.00");
            GUILayout.Label("Zoom: " + zoomValue + "x", EditorStyles.label);
            GUILayout.EndHorizontal();
        }

        private void CreateAsset()
        {
            samplesAssetPath = string.Empty;
            Data = CreateInstance<AudioxSampleLibraryAsset>();
            Data.hideFlags = HideFlags.HideAndDontSave;
        }

        private void LoadAsset(string path)
        {
            if (path != string.Empty)
            {
                var index = path.IndexOf("Assets", StringComparison.Ordinal);
                if (index > 0)
                {
                    path = path.Remove(0, index);
                }
                path = path.Replace(Application.dataPath, "Assets");
                samplesAssetPath = path;
                var previousAsset = loadedAsset;
                loadedAsset = AssetDatabase.LoadAssetAtPath<AudioxSampleLibraryAsset>(path);
                if (loadedAsset != null)
                {
                    UpdateDataAsset();
                    OnLoadData();
                    if (previousAsset != loadedAsset)
                    {
                        Undo.ClearAll();
                    }
                }
                else
                {
                    Debug.LogWarning("Can't load asset!");
                }
            }
        }
        
        private void UpdateDataAsset()
        {
            if (AutoSave)
            {
                Data = loadedAsset;
            }
            else
            {
                Data = CreateInstance<AudioxSampleLibraryAsset>();
                Data.hideFlags = HideFlags.HideAndDontSave;
                Data.Clip = loadedAsset.Clip;
                Data.LibraryName = loadedAsset.LibraryName;
                Data.Samples.Clear();
                
                foreach (var loadedSample in loadedAsset.Samples) 
                {
                    var newSamples = new Sample(loadedSample.StartPosition, loadedSample.EndPosition, loadedSample.Name, loadedSample.Description, loadedSample.Step, loadedSample.IsLastInStep);
                    Data.Samples.Add(newSamples);
                }
            }
        }

        private void SaveAsset()
        {
            var asset = CreateInstance<AudioxSampleLibraryAsset>();
            asset.Clip = Data.Clip;
            asset.LibraryName = Data.LibraryName;
            Data.Samples.Sort(delegate(Sample x, Sample y) { return x.StartPosition.CompareTo(y.StartPosition); });
            asset.Samples = Data.Samples;
            if (samplesAssetPath == string.Empty)
            {
                samplesAssetPath = EditorUtility.SaveFilePanelInProject("Asset", "Samples", "asset", "Choose path to save asset");
                if (samplesAssetPath != string.Empty)
                {
                    AssetDatabase.CreateAsset(asset, samplesAssetPath);
                    loadedAsset = asset;
                }
            }
            else
            {
                if (loadedAsset != null)
                {
                    EditorUtility.CopySerialized(asset, loadedAsset);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    Debug.Log("Can't save asset! Loaded asset was destroyed!");
                    samplesAssetPath = string.Empty;
                }
            }
        }

        private void DrawAudioCursor()
        {
            var xMin = audioManager.Source.time / Data.Clip.length * _curveRect.width + _curvePadding.x;
            var audioPositionRect = new Rect(xMin, EditorStyles.toolbar.fixedHeight, 2f, _curvePanel.height);
            EditorGUI.DrawRect(audioPositionRect, _audioCursorColor);
            var audioPosition = ExtraMethods.FormatTimeSpan(TimeSpan.FromSeconds(audioManager.Source.time), true);
            var audioLabelRect = new Rect(xMin + 4f, _curvePanel.height - ScrollHeight, 50f, 20f);
            var audioLabelStyle = EditorStyles.miniLabel;
            audioLabelStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUI.LabelField(audioLabelRect, audioPosition, audioLabelStyle);
        }

        private void DrawBottomPanel()
        {
            EditorGUI.BeginDisabledGroup(Data.Clip == null);
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent("Add Samples", "Add a new samples to timeline"), EditorStyles.toolbarButton))
            {
                var startTime = audioManager.Source.time;
                var endTime = Data.Clip.length;
                var timeOffset = Data.Clip.length / 16f;
                if (audioManager.Source.time + timeOffset <= Data.Clip.length)
                {
                    endTime = startTime + timeOffset;
                }
                else
                {
                    startTime -= timeOffset;
                }
                _samples.AddSample(startTime, endTime);
                _samples.Deselect();
                _samples.SelectedLastSampleAdded();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_samples.GetSelectedSamplesCount() == 0);
            var postfix = _samples.GetSelectedSamplesCount() > 1 ? "s" : string.Empty;
            if (GUILayout.Button(new GUIContent("Remove Samples" + postfix, "Remove selected Sample/s from timeline"), EditorStyles.toolbarButton) && _samples.GetSelectedSamplesCount() > 0)
            {
                var selectedMany = _samples.GetSelectedSamplesCount() > 1 && EditorUtility.DisplayDialog("Confirmation",
                    "Are you sure you want to remove selected samples?", "Yes", "Cancel");
                var selectedOne = _samples.GetSelectedSamplesCount() == 1 && EditorUtility.DisplayDialog("Confirmation",
                    "Are you sure you want to remove selected samples?", "Yes", "Cancel");
                if (selectedOne)
                {
                    Undo.RecordObject(Data, "Remove samples");
                    Data.Samples.Remove(_samples.GetFirstSelectedSample());
                }
                else if (selectedMany)
                {
                    Undo.RecordObject(Data, "Remove Samples");
                    var selectedSamples = _samples.GetSelectedSamples();
                    foreach (var sample in selectedSamples)
                    {
                        Data.Samples.Remove(sample);
                    }
                }
                if (selectedMany || selectedOne)
                {
                    _samples.Deselect();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(Data.Clip == null);
            EditorGUI.BeginChangeCheck();
            var playContent = EditorGUIUtility.IconContent("Animation.Play");
            playContent.tooltip = "Play current Audio Clip";
            if (audioManager.Source != null)
            {
                _isPlaying = GUILayout.Toggle(audioManager.Source.isPlaying, playContent, EditorStyles.toolbarButton);
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (_isPlaying)
                {
                    PlayClip();
                }
                else
                {
                    PauseClip();
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_samples.GetFirstSelectedSample() == null);
            var playSampleContent = EditorGUIUtility.IconContent("Animation.NextKey");
            playSampleContent.tooltip = "Play selected sample";
            if (GUILayout.Button(playSampleContent, EditorStyles.toolbarButton))
            {
                _audioPlayingSample = _samples.GetFirstSelectedSample();
                PlaySample();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void DrawSeparator()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAudioClipField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AudioClip:", GUILayout.Width(LabelWidth));
            EditorGUI.BeginChangeCheck();
            Data.Clip = (AudioClip) EditorGUILayout.ObjectField(Data.Clip, typeof(AudioClip), false, GUILayout.Width(position.width - LabelWidth - 12f));
            if (EditorGUI.EndChangeCheck())
            {
                audioManager.SetPosition(0f);
                OnLoadData();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnLoadData()
        {
            _curve.LoadData(Data.Clip);
            audioManager.Destroy();
            audioManager.InitSource(Data.Clip);
            if (_samples != null)
            {
                _samples.Deselect();
            }
            _curveSize = 1f;
        }

        private void DrawTextField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text:", EditorStyles.label);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var textAreaStyle = new GUIStyle(GUI.skin.textArea)
            {
                wordWrap = true,
                stretchHeight = true,
                stretchWidth = true,
                richText = true,
            };
            _scrollTextAreaPosition = EditorGUILayout.BeginScrollView(_scrollTextAreaPosition);
            EditorGUI.BeginChangeCheck();
            Data.LibraryName = EditorGUILayout.TextArea(Data.LibraryName, textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                _textAreaRect = GUILayoutUtility.GetLastRect();
            }
        }

        private void PlayClip()
        {
            if (audioManager.Source.time >= audioManager.Source.clip.length)
            {
                audioManager.Source.time = 0f;
            }
            audioManager.Source.clip = Data.Clip;
            audioManager.Source.Play();
            _isPlaying = true;
        }

        private void PauseClip()
        {
            audioManager.Source.Pause();
            _isPlaying = false;
        }

        private void PlaySample()
        {
            audioManager.Source.clip = Data.Clip;
            audioManager.Source.time = _audioPlayingSample.StartPosition;
            audioManager.Source.Play();
            _isPlaying = true;
            _isPlayingSample = true;
        }
        
        private void RestoreClipPosition(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                audioManager.RestorePosition();
            }
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Repaint();
            }
        }

        private float GetPixelPosition(float pos)
        {
            return pos / Data.Clip.length * _curveRect.width + _curvePadding.x;
        }

        public void Init(bool reInitialize = false, string assetPath = null)
        {
            Instance = GetWindow<AudioxEditorWindow>(Title, false);

            if (!string.IsNullOrEmpty(assetPath))
            {
                samplesAssetPath = assetPath;
            }
            
            if (reInitialize)
            {
                Data = null;
                _isDataLoaded = false;
                samplesAssetPath = string.Empty;
            }

            if (audioManager == null || reInitialize)
            {
                audioManager = new AudioxAudioManager();
            }

            if (_curve == null || reInitialize)
            {
                _curve = new Curve();
            }

            if (Data == null)
            {
                Data = CreateInstance<AudioxSampleLibraryAsset>();
                Data.hideFlags = HideFlags.HideAndDontSave;
                var selection = Selection.activeObject;
                if (selection != null && selection is AudioClip)
                {
                    Data.Clip = (AudioClip) selection;
                    OnLoadData();
                    _isDataLoaded = true;
                }
                else if (selection != null && selection is AudioxSampleLibraryAsset)
                {
                    samplesAssetPath = AssetDatabase.GetAssetPath(selection);
                    LoadAsset(samplesAssetPath);
                    if (_samples != null)
                    {
                        _samples.Deselect();
                    }
                    _isDataLoaded = true;
                }
            }

            if (audioManager.Source == null || reInitialize)
            {
                audioManager.InitSource(Data.Clip);
            }

            if (_timeline == null || reInitialize)
            {
                _timeline = new Timeline();
            }

            if (_samples == null || reInitialize)
            {
                _samples = new Samples();
            }

            if (_input == null || reInitialize)
            {
                _input = new Input
                {
                    OnMouseWheel = OnMouseWheel,
                    OnMouseDown = OnMouseDown,
                    OnMouseDrag = OnMouseDrag,
                    OnMouseUp = _samples.OnMouseUp,
                    OnRepaint = _samples.OnRepaint
                };
            }

            if (!_isDataLoaded && reInitialize)
            {
                _isDataLoaded = true;
                LoadAsset(samplesAssetPath);
            }

            _autoSave = EditorPrefs.GetBool("ATS_AutoSave", false);

            Undo.undoRedoPerformed -= Repaint;
            Undo.undoRedoPerformed += Repaint;
            EditorApplication.playModeStateChanged -= RestoreClipPosition;
            EditorApplication.playModeStateChanged += RestoreClipPosition;
        }

        public Rect GetPixelRect(float pos)
        {
            var pixelPosition = GetPixelPosition(pos);
            return new Rect(pixelPosition, EditorStyles.toolbar.fixedHeight, 2f, _curvePanel.height);
        }

        public Rect GetCurveRect()
        {
            return _curveRect;
        }

        public float GetPanelHeight()
        {
            return _curvePanel.height;
        }
    }
}