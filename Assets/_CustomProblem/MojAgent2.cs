using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

public class MojAgent2 : Agent
{
  // Referencje do komponentów fizycznych agenta
  private Rigidbody agentRigidbody;

  // Referencja do p³aszczyzny i celu
  public GameObject plane;
  public GameObject goal;

  // Zakres losowania pozycji agenta
  private Vector3 spawnAreaMin;
  private Vector3 spawnAreaMax;
  public float moveSpeed;

  private void Start()
  {
    agentRigidbody = GetComponent<Rigidbody>();

    // Oblicz granice p³aszczyzny
    MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
    spawnAreaMin = planeRenderer.bounds.min;
    spawnAreaMax = planeRenderer.bounds.max;
  }

  // Metoda wywo³ywana na pocz¹tku ka¿dego epizodu
  public override void OnEpisodeBegin()
  {
    // Wyzeruj stan fizyczny agenta
    ResetAgentPhysics();

    // Wylosuj now¹ pozycjê dla agenta
    Vector3 newPosition = GetRandomSpawnPosition();
    transform.position = newPosition;

    // Resetuj pozycjê celu
    goal.transform.position = GetRandomSpawnPosition();
  }

  // Metoda do resetowania stanu fizycznego agenta
  private void ResetAgentPhysics()
  {
    agentRigidbody.velocity = Vector3.zero;
    agentRigidbody.angularVelocity = Vector3.zero;
  }

  // Metoda do losowania nowej pozycji w okreœlonym zakresie
  private Vector3 GetRandomSpawnPosition()
  {
    float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
    float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
    float y = transform.position.y; // Zak³adamy, ¿e agent porusza siê po p³aszczyŸnie i y pozostaje sta³e
    
    return new Vector3(x, y, z);
  }

  // Metoda Heuristic do rêcznego sterowania agentem
  public override void Heuristic(in ActionBuffers actionsOut)
  {
    var continuousActionsOut = actionsOut.ContinuousActions;
    continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Przyk³adowe sterowanie poziome
    continuousActionsOut[1] = Input.GetAxis("Vertical"); // Przyk³adowe sterowanie pionowe
  }

  // Metoda do przetwarzania akcji
  public override void OnActionReceived(ActionBuffers actionBuffers)
  {
    var continuousActions = actionBuffers.ContinuousActions;

    // Przyk³adowe zastosowanie akcji do ruchu agenta
    float moveHorizontal = continuousActions[0];
    float moveVertical = continuousActions[1];

    // Zastosowanie akcji do Rigidbody
    Vector3 controlSignal = new Vector3(moveHorizontal, 0, moveVertical).normalized;
    agentRigidbody.AddForce(controlSignal * moveSpeed);

  }

  private void OnCollisionEnter(Collision collision)
  {
    float reward = 0.0f;
    tag = collision.gameObject.tag;
    if (tag == "Goal")
    {
      reward = 5.0f;
      Debug.Log($"Kolizja z '{tag}'");
      AddReward(reward);
      EndEpisode();
    }
    // SprawdŸ, czy kolizja dotyczy obiektu z tagiem "Wall"
    else if (tag == "Wall")
    {
      reward = -1.0f;
      Debug.Log($"Kolizja z '{tag}'");
      AddReward(reward);
      EndEpisode();
    }
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    // 1. Pozycja agenta
    sensor.AddObservation(transform.localPosition);

    // 2. Pozycja celu (jeœli jest okreœlona)
    if (goal != null)
    {
      sensor.AddObservation(goal.transform.localPosition);
    }
    else
    {
      // Jeœli cel nie jest zdefiniowany, dodaj zerow¹ pozycjê
      sensor.AddObservation(Vector3.zero);
    }

    // 3. Prêdkoœæ agenta
    // Mo¿esz zbieraæ równie¿ inne informacje dotycz¹ce prêdkoœci lub kierunku ruchu
    //sensor.AddObservation(agentRigidbody.velocity.magnitude);
    sensor.AddObservation(agentRigidbody.velocity.x);
    sensor.AddObservation(agentRigidbody.velocity.z);

    // Dodaj dodatkowe obserwacje wed³ug potrzeb, np. kierunek ruchu, rotacja itp.

    // Przyk³adowe dodatkowe obserwacje:
    // sensor.AddObservation(transform.forward); // Kierunek w którym patrzy agent
  }
}
