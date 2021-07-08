using UnityEngine;

public class BaseChannel : MonoBehaviour
{
    protected bool IsValueUpdated = false;
    public object Value
    {
        get 
        {
            return Value;
        }
        set 
        {
            if(Value != value && !IsValueUpdated)
            {
                Value = value;
                IsValueUpdated = true;
            }
        }
    }

    void Update()
    {
        if (IsValueUpdated)
            IsValueUpdated = false;
    }
}
