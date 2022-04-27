using UnityEngine;
using System.Collections.Generic;

public class SpriteManager:MonoBehaviour {

	public static System.Action OnAwake;

	[SerializeField]
	public Sprite[] spriteArray;

	private Dictionary<string, Sprite> _spriteDict = new Dictionary<string, Sprite>();

    public static SpriteManager instance {
		get; private set;
	}

	public void Awake() {
		if (instance == null) {
			instance = this;
			instance.LoadThumbnailTextureAtlases();
			DontDestroyOnLoad(this);
			if (OnAwake != null) {
				OnAwake();
			}
		}
		else {
			if (this != instance) {
				Destroy(this.gameObject);
			}
		}
	}

	public Sprite GetSprite(string itemID) {
		Sprite sprite = null;
		if (!_spriteDict.TryGetValue(itemID.ToString(), out sprite)) {
			Debug.LogWarning("SpriteManager: unable to find sprite '" + itemID.ToString() + "'");
			_spriteDict.TryGetValue("empty", out sprite);
		}
		return sprite;
	}

	private void LoadThumbnailTextureAtlases() {
		_spriteDict.Clear();
		for (int i = 0; i < spriteArray.Length; i++) {
			Sprite sprite = spriteArray[i];
			_spriteDict[sprite.name] = sprite;
		}
	}
}