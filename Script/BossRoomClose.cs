using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BossRoomClose : MonoBehaviour
{
    [SerializeField] private GameObject bossRoomDoor;
    [SerializeField] private GameObject bossRoomDoorCollision;
    [SerializeField] private CinemachineCamera bossCam;
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && collision.gameObject.transform.position.x > this.gameObject.transform.position.x)
        {
            CloseBossRoom();
        }
    }

    public void CloseBossRoom()
    {
        Debug.Log("Boss room is now closed!");
        
        bossRoomDoorCollision.SetActive(true);
        bossCam.Priority = 20;

        StartCoroutine(CloseDoorSprite());
    }

    private IEnumerator CloseDoorSprite()
    {
        yield return new WaitForSeconds(1.2f);
        bossRoomDoor.SetActive(true);

        yield return null;
    }
}
