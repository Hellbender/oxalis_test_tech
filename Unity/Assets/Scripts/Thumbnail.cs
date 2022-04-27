using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Thumbnail:MonoBehaviour {

	private ThumbnailVO _thumbnailVO; 
	
	public ThumbnailVO thumbnailVO {
		get {
			return _thumbnailVO;
		}
		set {
			_thumbnailVO = value;
			GetComponent<Image>().sprite = SpriteManager.instance.GetSprite(_thumbnailVO.id);
			GetComponentInChildren<Text>().text = _thumbnailVO.id.ToString();
		}
	}

	public void OnClick() {
		Debug.Log("Thumbnail clicked: "+_thumbnailVO.id);
	}
}