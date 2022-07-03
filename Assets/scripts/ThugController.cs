using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThugController : CharacterBase,ICharcterInteface
{
    public void TakeHit(Vector2 hitPosition)
    {
        Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        GameManager.instance.AddScore(1);
        GameManager.instance.ClearSpawnIndex(spawnIndex);
        Destroy(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(WaitAction), 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void WaitAction()
    {
        Debug.Log("shoot");
        //shoot up;
        GameManager.instance.TakeDamage();
        //move awaky;
        MoveAwayFromScreen(GameManager.instance.GetMoveAwayPos(spawnIndex));
    }
}
