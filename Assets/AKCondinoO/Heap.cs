using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Heap<T>where T:IHeapItem<T>{
}
public interface IHeapItem<T>:IComparable<T>{
int HeapIndex{get;set;}
}