using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

public class MojAgent2 : Agent
{
  // Referencje do komponent�w fizycznych agenta
  private Rigidbody agentRigidbody;

  // Referencja do p�aszczyzny i celu
  public GameObject plane;
  public GameObject goal;

  // Zakres losowania pozycji agenta
  private Vector3 spawnAreaMin;
  private Vector3 spawnAreaMax;
  public float moveSpeed;

  private void Start()
  {
    agentRigidbody = GetComponent<Rigidbody>();

    // Oblicz granice p�aszczyzny
    MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
    spawnAreaMin = planeRenderer.bounds.min;
    spawnAreaMax = planeRenderer.bounds.max;
  }

  // Metoda wywo�ywana na pocz�tku ka�dego epizodu
  public override void OnEpisodeBegin()
  {
    // Wyzeruj stan fizyczny agenta
    ResetAgentPhysics();

    // Wylosuj now� pozycj� dla agenta
    Vector3 newPosition = GetRandomSpawnPosition();
    transform.position = newPosition;

    // Resetuj pozycj� celu
    goal.transform.position = GetRandomSpawnPosition();
  }

  // Metoda do resetowania stanu fizycznego agenta
  private void ResetAgentPhysics()
  {
    agentRigidbody.velocity = Vector3.zero;
    agentRigidbody.angularVelocity = Vector3.zero;
  }

  // Metoda do losowania nowej pozycji w okre�lonym zakresie
  private Vector3 GetRandomSpawnPosition()
  {
    float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
    float z = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
    float y = transform.position.y; // Zak�adamy, �e agent porusza si� po p�aszczy�nie i y pozostaje sta�e
    
    return new Vector3(x, y, z);
  }

  // Metoda Heuristic do r�cznego sterowania agentem
  public override void Heuristic(in ActionBuffers actionsOut)
  {
    var continuousActionsOut = actionsOut.ContinuousActions;
    continuousActionsOut[0] = Input.GetAxis("Horizontal"); // Przyk�adowe sterowanie poziome
    continuousActionsOut[1] = Input.GetAxis("Vertical"); // Przyk�adowe sterowanie pionowe
  }

  // Metoda do przetwarzania akcji
  public override void OnActionReceived(ActionBuffers actionBuffers)
  {
    var continuousActions = actionBuffers.ContinuousActions;

    // Przyk�adowe zastosowanie akcji do ruchu agenta
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
    // Sprawd�, czy kolizja dotyczy obiektu z tagiem "Wall"
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

    // 2. Pozycja celu (je�li jest okre�lona)
    if (goal != null)
    {
      sensor.AddObservation(goal.transform.localPosition);
    }
    else
    {
      // Je�li cel nie jest zdefiniowany, dodaj zerow� pozycj�
      sensor.AddObservation(Vector3.zero);
    }

    // 3. Pr�dko�� agenta
    // Mo�esz zbiera� r�wnie� inne informacje dotycz�ce pr�dko�ci lub kierunku ruchu
    //sensor.AddObservation(agentRigidbody.velocity.magnitude);
    sensor.AddObservation(agentRigidbody.velocity.x);
    sensor.AddObservation(agentRigidbody.velocity.z);

    // Dodaj dodatkowe obserwacje wed�ug potrzeb, np. kierunek ruchu, rotacja itp.

    // Przyk�adowe dodatkowe obserwacje:
    // sensor.AddObservation(transform.forward); // Kierunek w kt�rym patrzy agent
  }
}
