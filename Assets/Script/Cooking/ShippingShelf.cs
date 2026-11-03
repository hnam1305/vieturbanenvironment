using UnityEngine;

public class ShippingShelf : MonoBehaviour
{
    [SerializeField] private Transform[] slots;
    [SerializeField] private ShippingBox prefab;
    private ShippingBox[] boxes;
    private void Awake()
    {
        boxes= new ShippingBox[slots.Length];
    }
    public int FindFreeSlot() //return index of available slot
    {
        for (int i = 0; i < boxes.Length; i++)
        {
            if (boxes[i] == null && slots[i] != null)
            {
                return i;
            }
        }
        return -1;
    }
    public bool TryCreateBox(Order order) //create box for one order
    {
        if (order == null)
        {
            return false;
        }
        if (prefab == null)
        {
            return false;
        }
        if (order.OrderStatus != Order.OrderState.Closed)
        {
            return false;
        }
        if (order.Recipe == null)
        {
            return false;
        }
        int index = FindFreeSlot();
        if (index == -1)
        {
            return false;
        }
        if (order.MarkBoxCreated() == false)
        {
            return false;
        }
        ShippingBox box = Instantiate(prefab, slots[index].position, slots[index].rotation);
        if (box.SetUp(order, this, index) == false)
        {
            Destroy(box.gameObject);
            Debug.Log("set up fail");
            return false;
        }
        boxes[index] = box;
        return true;

    }
    public void ReleaseSlot(int index)
    {
        if (index < 0 || index >= boxes.Length)
        {
            return;
        }
        boxes[index] = null;
        Debug.Log("Released shelf slot: " + index);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
