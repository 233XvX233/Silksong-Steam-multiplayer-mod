using UnityEngine;

public class SkinLock : MonoBehaviour
{
    public tk2dSpriteCollectionData Cloned;
    public Texture2D[] Atlases; // 每个 material 对应一张贴图（可选）
    public int atlasesid = 0;
    public string dataName = "";

    tk2dBaseSprite _sprite;
    Renderer _r;

    void Awake()
    {
        _sprite = GetComponent<tk2dBaseSprite>();
        _r = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (!_sprite || Cloned == null) return;
        atlasesid = Skin.GetCurrentAtlas(_sprite);
        if (_sprite.Collection.name == dataName)
        {
            // 1) 硬锁 Collection（动画改回去就再改回来）
            if (_sprite.Collection != Cloned)
                _sprite.Collection = Cloned;


            // 2) 硬锁材质贴图（更关键：很多时候是 material 被改回）
            if (_r && Atlases != null && Atlases.Length > 0)
            {
                var tex = Atlases[atlasesid];
                if (!tex) return;

                // ✅ 用 material：只影响当前 Renderer
                var mat = _r.material;
                if (mat != null && mat.mainTexture != tex)
                    mat.mainTexture = tex;
            }
        }
    }
}
