using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThugController : CharacterBase,ICharcterInteface
{
    public Sprite ShootSprite;
    public void TakeHit(Vector2 hitPosition)
    {
        Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        GameManager.instance.AddScore(1);
        PlayHurtMotion();
        //Destroy(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        //Invoke(nameof(WaitAction), 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void WaitAction()
    {
        StopCoroutine(nameof(ShootAction));
        StartCoroutine(nameof(ShootAction));
    }

    IEnumerator ShootAction()
    {
        GetComponent<SpriteRenderer>().sprite = ShootSprite;
        transform.localScale = new Vector2(1.18f, 1.12f);
        soundManager.instance.PlaySound(SoundType.AkShoot);
        yield return new WaitForSeconds(0.2f);
        GameManager.instance.TakeDamage();
        yield return new WaitForSeconds(0.2f);
        MoveAwayFromScreen(GameManager.instance.GetMoveAwayPos(spawnIndex));
    }
}
