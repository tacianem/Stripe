using UnityEngine;

public class Cane : MonoBehaviour {
    public Sprite newSprite;
    private SpriteRenderer sr;
    public bool striped = false;

    void Start() {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ChangeSprite() {
        sr.sprite = newSprite;
        striped = true;
    }

}