using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Drawing;


public partial class SceneObjectReferencer : EditorWindow
{
    [MenuItem("Window/SceneObjectReferencer")]
    private static void ShowWindowFromMainMenu()
    {
        var window = GetWindow<SceneObjectReferencer>("UIElements");
        // 拡張ウィンドウのタイトル設定
        window.titleContent = new GUIContent("SceneObjectReferencer");
        // ウィンドウの表示
        window.Show();
    }

    [MenuItem("GameObject/SceneObjectReferencer")]
    private static void ShowWindowFromRightClick()
    {
        var window = GetWindow<SceneObjectReferencer>("UIElements");
        // 拡張ウィンドウのタイトル設定
        window.titleContent = new GUIContent("SceneObjectReferencer");
        // 右クリックしたオブジェクトをObjectFieldに設定するために取得
        selectedGameObject = Selection.activeTransform.gameObject;
        // ウィンドウの表示
        window.Show();
    }

    [SerializeField] private VisualTreeAsset _rootVisualTreeAsset;
    [SerializeField] private StyleSheet _rootStyleSheet;

    private static GameObject selectedGameObject;
    private Object _sourceObject;
    private Dictionary<string, Component> _sourceComponents = new Dictionary<string, Component>();
    private List<GameObject> _searchResult = new List<GameObject>();
    private List<Component> _searchResultComponents = new List<Component>();
    private List<Component> _selectedComponents = new List<Component>();
    private GameObject _selectedGameObject;
    private bool _isAllSelected;
    private bool _isFoldoutOpened;

    private async void CreateGUI()
    {
        _rootVisualTreeAsset.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(_rootStyleSheet);
        EditorLocalization editorLocalization = ScriptableObject.CreateInstance<EditorLocalization>();
        editorLocalization.GetTable();

        // コンポーネント表示のボタンのクリックイベント
        // ObjectField内のオブジェクトにアタッチされているコンポーネントを表示する
        var showComponentButton = rootVisualElement.Q<Button>("ShowComponentButton");
        // ボタンをクリックできないようにする
        showComponentButton.SetEnabled(false);
        
        // コンポーネント表示のボタンのクリックイベント
        showComponentButton.clickable.clicked += () =>
        {
            ShowComponents();
            rootVisualElement.Q<VisualElement>("SearchOptions").style.display = DisplayStyle.Flex;
        };

        var searchButton = rootVisualElement.Q<Button>("SearchButton");
        // ボタンをクリックできないようにする
        searchButton.SetEnabled(false);
        // 検索ボタンのクリックイベント
        searchButton.clickable.clicked += () =>
        {
            Search();
        };

    }

    private void ShowComponents()
    {
        _sourceObject = rootVisualElement.Q<ObjectField>("SourceObjectField").value;
        var sourceComponentGroup = rootVisualElement.Q<RadioButtonGroup>("SourceComponentsGroup");
        var isIncludeInactiveComponent = rootVisualElement.Q<Toggle>("IsIncludeInactiveComponent").value;
        // 初期化
        sourceComponentGroup.Clear();
        _sourceComponents.Clear();

        // 全て選択するトグルを作成
        var allButton = new Button{ text = "すべて選択" };
        allButton.style.width = 60;
        allButton.clickable.clicked += () =>
        {
            sourceComponentGroup.Query<Toggle>().ForEach(toggle =>
            {
                toggle.value = true;
            });
        };
        sourceComponentGroup.Add(allButton);

        // ゲームオブジェクトのトグルを作成
        var toggle = new Toggle("GameObject");
        toggle.style.flexDirection = FlexDirection.Row;
        toggle.style.unityTextAlign = TextAnchor.MiddleLeft;
        toggle.RegisterValueChangedCallback(evt =>
        {
            if(evt.newValue)
            {
                _selectedGameObject = (GameObject)_sourceObject;
            }
            else
            {
                _selectedGameObject = null;
            }
        });

        sourceComponentGroup.Add(toggle);

        // コンポーネントのトグルを作成
        foreach (Component component in ((GameObject)_sourceObject).GetComponents<Component>())
        {
            var behaviourComponent = component as Behaviour;
            if(behaviourComponent != null)
            {
                if(!isIncludeInactiveComponent && behaviourComponent.enabled == false)
                {
                    continue;
                }
            }

            if(_sourceComponents.ContainsKey(component.GetType().Name))
            {
                continue;
            }
            
            _sourceComponents.Add(component.GetType().Name, component);

            toggle = new Toggle(component.GetType().Name);
            toggle.style.flexDirection = FlexDirection.Row;
            toggle.style.unityTextAlign = TextAnchor.MiddleLeft;
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    // トグルがオンになった場合
                    // 選択されたコンポーネントを取得
                    _selectedComponents.Add(_sourceComponents[component.GetType().Name]);
                }
                else
                {
                    // トグルがオフになった場合
                    // 選択されたコンポーネントを削除
                    _selectedComponents.Remove(_sourceComponents[component.GetType().Name]);
                }

            });

            sourceComponentGroup.Add(toggle);

        }
        
    }

    private void Search()
    {
        var sourceObj = rootVisualElement.Q<ObjectField>("SourceObjectField").value;
        var isIncludeInactiveObject = rootVisualElement.Q<Toggle>("IsIncludeInactiveObject").value;
        var isIncludeSourceObject = rootVisualElement.Q<Toggle>("IsIncludeSourceObject").value;

        rootVisualElement.Q<VisualElement>("SearchResultArea").Clear();

        if(_selectedGameObject != null)
        {
            foreach (GameObject obj in FindObjectsOfType(typeof(GameObject), isIncludeInactiveObject))
            {
                if(obj != sourceObj || isIncludeSourceObject)
                {
                    // objにアタッチされているコンポーネントを取得
                    foreach(var component in obj.GetComponents<Component>())
                    {
                        var serializedComponent = new SerializedObject(component);
                        var property = serializedComponent.GetIterator();
                        while (property.NextVisible(true))
                        {
                            if (property.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                if (property.objectReferenceValue == _selectedGameObject)
                                {
                                    _searchResult.Add(obj);
                                    _searchResultComponents.Add(component);
                                }
                            }
                        }
                    }
                }
            }

            ShowSearchResult("GameObject", _searchResult, _searchResultComponents);
            _searchResult.Clear();
            _searchResultComponents.Clear();
        }

        if(_isAllSelected)
        {
            _selectedComponents = new List<Component>(_sourceComponents.Values);
        }

        foreach(var targetComponent in _selectedComponents)
        {
            foreach (GameObject obj in FindObjectsOfType(typeof(GameObject), isIncludeInactiveObject))
            {
                if(obj != sourceObj || isIncludeSourceObject)
                {
                    // objにアタッチされているコンポーネントを取得
                    foreach(var component in obj.GetComponents<Component>())
                    {
                        var serializedComponent = new SerializedObject(component);
                        var property = serializedComponent.GetIterator();
                        while (property.NextVisible(true))
                        {
                            if (property.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                if (property.objectReferenceValue == targetComponent)
                                {
                                    _searchResult.Add(obj);
                                    _searchResultComponents.Add(component);
                                }
                            }
                        }

                    }
                }
            }

            ShowSearchResult(targetComponent.GetType().Name ,_searchResult, _searchResultComponents);
            _searchResult.Clear();
            _searchResultComponents.Clear();
        }
        
    }

    private void ShowSearchResult(string componentName, List<GameObject> searchResult, List<Component> searchResultComponents)
    {
        // 検索結果を表示するFoldoutを作成し、表示
        var searchResultFoldout = new Foldout();
        rootVisualElement.Q<VisualElement>("SearchResultArea").Add(searchResultFoldout);
        
        searchResultFoldout.text = $"{componentName}の検索結果";
        if(searchResult.Count > 0)
        {
            searchResultFoldout.value = true; // Foldoutを開いた状態にする
        }
        else
        {
            searchResultFoldout.value = false; // Foldoutを閉じた状態にする
            searchResultFoldout.text += " - 該当なし";
        }
        

        for(int i = 0; i < searchResult.Count; i++)
        {
            var visualElement = SetIconAndButton(searchResult[i], searchResultComponents[i]);

            searchResultFoldout.contentContainer.Add(visualElement);
        }
    }

    private VisualElement SetIconAndButton(GameObject searchResult, Component searchResultComponent)
    {
        var obj = searchResult;
        var visualElement = new VisualElement();
        visualElement.style.flexDirection = FlexDirection.Row;
        var icon = new VisualElement();
        icon.style.width = 20;
        icon.style.height = 20;
        visualElement.Add(icon);
        icon.style.backgroundImage = AssetPreview.GetMiniThumbnail(searchResultComponent);

        var button = new Button{ text = obj.name };
        button.style.width = 200;
        button.style.height = 20;
        visualElement.Add(button); 
        button.clickable.clicked += () =>
        {
            Selection.activeObject = obj;
        };

        return visualElement;
    }

    private void OnGUI()
    {
        if(selectedGameObject != null)
        {
            rootVisualElement.Q<ObjectField>("SourceObjectField").value = selectedGameObject;
            selectedGameObject = null;
        }
        
        if(rootVisualElement.Q<ObjectField>("SourceObjectField").value != null)
        {
            rootVisualElement.Q<Button>("ShowComponentButton").SetEnabled(true);
        }
        else
        {
            rootVisualElement.Q<Button>("ShowComponentButton").SetEnabled(false);
        }

        if(_selectedComponents.Count > 0 || _selectedGameObject != null)
        {
            rootVisualElement.Q<Button>("SearchButton").SetEnabled(true);
        }
        else
        {
            rootVisualElement.Q<Button>("SearchButton").SetEnabled(false);
        }
    }
}
