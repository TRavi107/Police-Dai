using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostageController : CharacterBase,ICharcterInteface
{
    public void TakeHit(Vector2 hitPosition)
    {
        Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        GameManager.instance.DecreaseLife();
        GameManager.instance.ClearSpawnIndex(spawnIndex);
        //Hurt effect
        Destroy(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void WaitAction()
    {
        //move awaky;
        MoveAwayFromScreen(GameManager.instance.GetMoveAwayPos(spawnIndex));
        Debug.Log("move awaky");
    }
    

}
