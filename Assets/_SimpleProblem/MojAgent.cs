using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[System.Serializable]
public class  Rewards
{
  public float sum;
  [Space]
  [SerializeField] float OnBramka;
  [SerializeField] float OnSciana;
  [SerializeField] float OnPlayer;
  [SerializeField] float OnPlayerWall;
  [SerializeField] float OnPlayerGoal;

  public float GetOnBramka()
  {
    sum += OnBramka;
    return OnBramka;
  }

  public float GetOnSciana()
  {
    sum += OnSciana;
    return OnSciana;
  }

  public float GetOnPlayer()
  {
    sum += OnPlayer;
    return OnPlayer;
  }

  public float GetOnPlayerWall()
  {
    sum += OnPlayerWall;
    return OnPlayerWall;
  }
  public float GetOnPlayerGoal()
  {
    sum += OnPlayerGoal;
    return OnPlayerGoal;
  }

}

public class MojAgent : Agent
{
  // Referencje do komponentów fizycznych agenta
  private Rigidbody agentRigidbody;
  private Rigidbody rbBall;

  // Referencja do p³aszczyzny i celu
  public GameObject plane;
  public GameObject plane1;
  public GameObject ballObj;
  public Ball ball;

  // Zakres losowania pozycji agenta
  private Vector3 spawnAreaMin;
  private Vector3 spawnAreaMax;
  private Vector3 spawnAreaMin1;
  private Vector3 spawnAreaMax1;

  public float moveSpeed;
  public Rewards rewards = new();

  private void Start()
  {
    agentRigidbody = GetComponent<Rigidbody>();
    rbBall = ballObj.GetComponent<Rigidbody>();

    // Oblicz granice p³aszczyzny
    MeshRenderer planeRenderer = plane.GetComponent<MeshRenderer>();
    spawnAreaMin = planeRenderer.bounds.min;
    spawnAreaMax = planeRenderer.bounds.max;
    //Ball
    MeshRenderer planeRenderer1 = plane1.GetComponent<MeshRenderer>();
    spawnAreaMin1 = planeRenderer1.bounds.min;
    spawnAreaMax1 = planeRenderer1.bounds.max;
  }
  private Vector3 GetRandomSpawnPosition(Vector3 spawnMin, Vector3 spawnMax)
  {
    float x = Random.Range(spawnMin.x, spawnMax.x);
    float z = Random.Range(spawnMin.z, spawnMax.z);
    float y = transform.position.y; // Zak³adamy, ¿e agent porusza siê po p³aszczyŸnie i y pozostaje sta³e

    return new Vector3(x, y, z);
  }

  private void Awake()
  {
    ball.onBramka += OnBramka;
    ball.onSciana += OnSciana;
    ball.onPlayer += OnPlayer;
  }
  private void OnDestroy()
  {
    ball.onBramka -= OnBramka;
    ball.onSciana -= OnSciana;
    ball.onPlayer -= OnPlayer;
  } 
  private void OnPlayer()
  {
    AddReward(rewards.GetOnPlayer());
  }

  private void OnSciana()
  {
    AddReward(rewards.GetOnSciana());
    EndEpisode();
  }

  private void OnPlayerWall()
  {
    AddReward(rewards.GetOnPlayerWall());
    EndEpisode();
  }

  private void OnPlayerGoal()
  {
    AddReward(rewards.GetOnPlayerGoal());
   
  }
  private void OnBramka()
  {
    AddReward(rewards.GetOnBramka());
    EndEpisode();
  }

  // Metoda wywo³ywana na pocz¹tku ka¿dego epizodu
  public override void OnEpisodeBegin()
  {
    rewards.sum = 0;
    // Wyzeruj stan fizyczny agenta
    agentRigidbody.velocity = Vector3.zero;
    agentRigidbody.angularVelocity = Vector3.zero;

    // Wylosuj now¹ pozycjê dla agenta
    Vector3 newPosition = GetRandomSpawnPosition(spawnAreaMin, spawnAreaMax);
    transform.position = newPosition;

    // Resetuj pozycjê i predkosc pilki
    rbBall.velocity = Vector3.zero;
    rbBall.angularVelocity = Vector3.zero;

    ballObj.transform.position = GetRandomSpawnPosition(spawnAreaMin1, spawnAreaMax1);
  }

  // Metoda do losowania nowej pozycji w okreœlonym zakresie


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

    float moveHorizontal = continuousActions[0];
    float moveVertical = continuousActions[1];

    transform.position += new Vector3(moveHorizontal, 0, moveVertical) * Time.deltaTime * moveSpeed;
  }

  private void OnCollisionEnter(Collision collision)
  {
    if(collision.collider.tag == "Wall")
    {
      OnPlayerWall();
    }
    if (collision.collider.tag == "Goal")
    {
      OnPlayerGoal();
    }
  }


  public override void CollectObservations(VectorSensor sensor)
  {
    sensor.AddObservation(transform.localPosition);
    sensor.AddObservation(ballObj.transform.localPosition);
    sensor.AddObservation(ball.transform.localPosition);
  }
}
