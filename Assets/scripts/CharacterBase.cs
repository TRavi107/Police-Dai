using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    public int spawnIndex;
    public GameObject hitEffectPrefab;
    protected bool inPosition;
    public Sprite hurtSprite;
    IEnumerator Cour_MoveTowardsScreen(Vector3 position)
    {
        while(Vector3.Distance(transform.position,position) > 0.01)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, 5 * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(3);
        WaitAction();
    }
    IEnumerator Cour_MoveAwayFromScreen(Vector3 position)
    {
        while (Vector3.Distance(transform.position, position) > 0.01)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, 5 * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(3);
        MoveAwayAction();
    }

    public void MoveTowardsScreen(Vector3 destination)
    {
        StartCoroutine(nameof(Cour_MoveTowardsScreen), destination);
    }

    public void MoveAwayFromScreen(Vector3 destination)
    {
        StartCoroutine(nameof(Cour_MoveAwayFromScreen), destination);
    }
    public void MoveAwayAction()
    {
        GameManager.instance.ClearSpawnIndex(spawnIndex);
        Destroy(this.gameObject);
        GameManager.instance.ClearSpawnIndex(spawnIndex);

    }
    public void PlayHurtMotion()
    {
        StopCoroutine(nameof(Cour_MoveTowardsScreen));
        StopCoroutine(nameof(Cour_MoveAwayFromScreen));
        GetComponent<SpriteRenderer>().sprite = hurtSprite;
        Invoke(nameof(MoveAwayAction), 0.3f);
    }
    public virtual void WaitAction() { }
}
