using System;

// As Unity does not support the use of Dictionary in the inspector, we create a custom class to be able to use it.

[Serializable]
public class CustomDictionary
{
    public int numberOfItems;
    public BaseItem baseItem;
}