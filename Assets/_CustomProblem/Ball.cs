using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
  public Action onBramka;
  public Action onSciana;
  public Action onPlayer;
  private void OnCollisionEnter(Collision collision)
  {
      if (collision.collider.tag == "Wall")
      {
        onSciana?.Invoke();
      }

      if (collision.collider.tag == "Goal")
      {
        onBramka?.Invoke();
      }

      if (collision.collider.tag == "Player")
      {
        onPlayer?.Invoke();
      }

  }

}
