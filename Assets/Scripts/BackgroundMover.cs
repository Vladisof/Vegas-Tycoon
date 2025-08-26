using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private bool autoStart = true;
    
    [Header("Optional Settings")]
    [SerializeField] private bool useLocalPosition = false;
    [SerializeField] private float pauseAtEnd = 0f;
    
    private bool isMoving = false;
    private float pauseTimer = 0f;
    
    private void Start()
    {
        if (autoStart)
        {
            StartMovement();
        }
        
        // Встановлюємо початкову позицію в точці A
        if (pointA != null)
        {
            SetPosition(pointA.position);
        }
    }
    
    private void Update()
    {
        if (isMoving)
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.deltaTime;
                return;
            }
            
            MoveTowardsTarget();
        }
    }
    
    private void MoveTowardsTarget()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("BackgroundMover: Point A or Point B is not assigned!");
            return;
        }
        
        Vector3 targetPosition = pointB.position;
        Vector3 currentPosition = GetCurrentPosition();
        
        // Рухаємося до цільової точки
        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, movementSpeed * Time.deltaTime);
        SetPosition(newPosition);
        
        // Перевіряємо, чи досягли цільової точки
        if (Vector3.Distance(newPosition, targetPosition) < 0.01f)
        {
            OnReachedTarget();
        }
    }
    
    private void OnReachedTarget()
    {
        // Телепортуємося назад в точку A
        SetPosition(pointA.position);
    }
    
    private Vector3 GetCurrentPosition()
    {
        return useLocalPosition ? transform.localPosition : transform.position;
    }
    
    private void SetPosition(Vector3 position)
    {
        if (useLocalPosition)
        {
            transform.localPosition = position;
        }
        else
        {
            transform.position = position;
        }
    }
    
    public void StartMovement()
    {
        isMoving = true;
    }
    
    public void StopMovement()
    {
        isMoving = false;
    }
    
    public void ResetToPointA()
    {
        if (pointA != null)
        {
            SetPosition(pointA.position);
        }
    }
    
    public void SetMovementSpeed(float speed)
    {
        movementSpeed = speed;
    }
    
    public void SetPoints(Transform newPointA, Transform newPointB)
    {
        pointA = newPointA;
        pointB = newPointB;
    }
}
