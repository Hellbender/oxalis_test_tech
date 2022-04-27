using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SpriteManagerImporter:AssetPostprocessor {

	private static string SPRITE_MANAGER_PATH = "Assets/Prefabs/SpriteManager.prefab";
	private static string PARENT_DIR = "Sprites";
	private static string BUNDLE_PARENT = "ui/sprites/";

	private static bool _createdPrefab = false;

	[MenuItem("Tools/Sprite Manager Update %#g")]
	public static void UpdateAllSprites() {
		SpriteManager spriteManager = GetSpriteManager();
		spriteManager.spriteArray = LoadSpriteLookup(PARENT_DIR + "/");

		EditorUtility.SetDirty(spriteManager.gameObject);
		if (_createdPrefab) {
			Editor.DestroyImmediate(spriteManager.gameObject);
			EditorWindow focus = EditorWindow.focusedWindow;
			focus.ShowNotification(new GUIContent("SpriteManager created, please set the AssetBundle name 'ui/sprites/spritemanager.unity3d"));
		}
		_createdPrefab = false;
	}

	void OnPostprocessTexture(Texture2D texture) {
		if (ValidFile(this.assetPath)) {
			TextureImporter importer = (TextureImporter)this.assetImporter;
			if (importer) {
				importer.textureType = TextureImporterType.Sprite;
				importer.textureCompression = TextureImporterCompression.Uncompressed;
				importer.spritePackingTag = "thumbnails";
			}
			this.assetImporter.assetBundleName = (BUNDLE_PARENT + "thumbnails.unity3d").ToLower();
		}
	}

	private static SpriteManager GetSpriteManager() {
		SpriteManager spriteManager = AssetDatabase.LoadAssetAtPath<SpriteManager>(SPRITE_MANAGER_PATH);
		if (spriteManager == null) {
			GameObject spriteManagerObj = new GameObject("SpriteManager", new System.Type[1] { typeof(SpriteManager) });
			PrefabUtility.CreatePrefab(SPRITE_MANAGER_PATH, spriteManagerObj);
			spriteManager = spriteManagerObj.GetComponent<SpriteManager>();
			_createdPrefab = true;
		}
		return spriteManager;
	}

	private static bool ValidFile(string file) {
		string dirName = Path.GetDirectoryName(file);
        return dirName == string.Format("Assets/{0}", PARENT_DIR);
	}

	private static Sprite[] LoadSpriteLookup(string path) {
		return GetAtPath<Sprite>(path);
	}

	public static T[] GetAtPath<T>(string path) {

		ArrayList al = new ArrayList();
		string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

		foreach (string fileName in fileEntries) {
			int assetPathIndex = fileName.IndexOf("Assets");
			string localPath = fileName.Substring(assetPathIndex);

			Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

			if (t != null)
				al.Add(t);
		}
		T[] result = new T[al.Count];
		for (int i = 0; i < al.Count; i++)
			result[i] = (T)al[i];

		return result;
	}
}