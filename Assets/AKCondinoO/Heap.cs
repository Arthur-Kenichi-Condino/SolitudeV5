using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pathfinder;
public class Heap<T>where T:IHeapItem<T>{
public bool LOG=false;public int LOG_LEVEL=1;
	  public Heap(int maxHeapSize){
items=new T[maxHeapSize];
      }
readonly T[]items;public int Count{get{return currentItemsCount;}}
										  int currentItemsCount;//  Because the items array is not always full
public bool Contains(T item){
return Equals(items[item.HeapIndex],item);
}
public void Add(T item){
item.HeapIndex=currentItemsCount;
items[currentItemsCount]=item;
SortUp(item);//  Teste os valores Parent deste item e mova o item na array até que ele tenha um Parent menor e (dois) Child maior(es) 
currentItemsCount++;
}
public void UpdateItem(T item){
SortUp(item);
}
/// <summary>
///  Se o valor Child for 6, Parent sempre será (int)((6-1)/2)=2
///  Fique em Loop até que o valor do item tenha os seguintes atributos: seu Parent tem que ser menor; seus Childs, maiores
/// </summary>
void SortUp(T item){
if(LOG&&LOG_LEVEL<=-100)Debug.Log("T item["+item.HeapIndex+"];_compare_item_with_parents'_indices_and_sort_");
_Loop:{
var parentIdx=(item.HeapIndex-1)/2;//  Se o valor Child for 6, Parent sempre será (int)((6-1)/2)=2
var parentItm=items[parentIdx];
if(item.CompareTo(parentItm)>0){//  Parent tem valor maior, trocar: colocar o Parent (menor prioridade) para baixo e o Child (maior prioridade) para cima 
if(LOG&&LOG_LEVEL<=-110)Debug.Log("_parent[index:"+parentItm.HeapIndex+((parentItm is Node)?",(Node.F:"+(parentItm as Node).F+"),(Node.H:"+(parentItm as Node).H+")":(""))+ "]_>_item[index:"+item.HeapIndex+((item is Node)?",(Node.F:"+(item as Node).F+"),(Node.H:"+(item as Node).H+")":"")+"]_:_item_has_higher_priority_;_pull_item_up_;");
Swap(item,parentItm);
}else{
if(LOG&&LOG_LEVEL<=-110)Debug.Log("_parent[index:"+parentItm.HeapIndex+((parentItm is Node)?",(Node.F:"+(parentItm as Node).F+"),(Node.H:"+(parentItm as Node).H+")":(""))+"]_<=_item[index:"+item.HeapIndex+((item is Node)?",(Node.F:"+(item as Node).F+"),(Node.H:"+(item as Node).H+")":"")+"]_:_item_has_equal_or_lower_priority_;_stop_moving_item_;");
goto _End;
}
goto _Loop;
}
_End:{}
}
/// <summary>
///  Adquire o valor no topo da Array 'items' segundo as otimizações de Heap e reposiciona os valores para não deixar um espaço vazio
/// </summary>
/// <returns></returns>
public T RemoveFirst(){
T firstItem=items[0];
//  The last temporarily becomes the first (then sort down the first item)
			   currentItemsCount--;//  [One item was removed]
items[0]=items[currentItemsCount];
items[0].HeapIndex=0;
//  Sort down até que o item tenha um Parent menor e Child(s) maior(es)
SortDown(items[0]);
return firstItem;}
void SortDown(T item){
if(LOG&&LOG_LEVEL<=-100)Debug.Log("T item["+item.HeapIndex+"];_compare_item_with_children's_indices_and_sort_");
_Loop:{
var indexToSwap=0;
var chldIdxLft=item.HeapIndex*2+1;//  Se o valor de Parent for 2, o Child (left ) sempre será 2*2+1=5
var chldIdxRgt=item.HeapIndex*2+2;//  Se o valor de Parent for 2, o Child (right) sempre será 2*2+2=6
if(chldIdxLft<currentItemsCount){//  Se existe um Child à esquerda,
indexToSwap=chldIdxLft;          // tentar dar Swap nele...
if(chldIdxRgt<currentItemsCount){//  Mas se existe um Child à direita,
                                 // compare os dois.
if(items[chldIdxLft].CompareTo(items[chldIdxRgt])<0){//  Se o Child da esquerda for maior (menor prioridade) que o da direita,
indexToSwap=chldIdxRgt;                              // então o Swap deve ser com o da direita (maior prioridade)
}
}
//  Tentar Swap
if(item.CompareTo(items[indexToSwap])<0){//  Se o item para Sort Down tem menor prioridade (é maior) que seu Child para Swap,
if(LOG&&LOG_LEVEL<=-110)Debug.Log("T item["+item.HeapIndex+"];_child[index:"+items[indexToSwap].HeapIndex+((items[indexToSwap] is Node)?",(Node.F:"+(items[indexToSwap] as Node).F+"),(Node.H:"+(items[indexToSwap] as Node).H+")":(""))+"]_<=_item[index:"+item.HeapIndex+((item is Node)?",(Node.F:"+(item as Node).F+"),(Node.H:"+(item as Node).H+")":"")+"]_:_item_has_equal_or_lower_priority_;_push_item_down_");
Swap(item,items[indexToSwap]);			 // realizar o Swap para baixo
}else{
if(LOG&&LOG_LEVEL<=-110)Debug.Log("T item["+item.HeapIndex+"];_child[index:"+items[indexToSwap].HeapIndex+((items[indexToSwap] is Node)?",(Node.F:"+(items[indexToSwap] as Node).F+"),(Node.H:"+(items[indexToSwap] as Node).H+")":(""))+ "]_>_item[index:"+item.HeapIndex+((item is Node)?",(Node.F:"+(item as Node).F+"),(Node.H:"+(item as Node).H+")":"")+"]_:_item_has_higher_priority_;_stop_moving_item_");
goto _End;//  Child them menor prioridade (é maior), então não realizar mais Swaps
}
}else{
if(LOG&&LOG_LEVEL<=-110)Debug.Log("T item["+item.HeapIndex+"];_no_(more)_children_to_try_to_swap_with_");
goto _End;//  Não há mais Childs para dar Swap
}
goto _Loop;
}
_End:{}
}
void Swap(T itemA,T itemB){
items[itemA.HeapIndex]=itemB;
items[itemB.HeapIndex]=itemA;
int itemA_HeapIndex=itemA.HeapIndex;//  Não se esqueça de trocar o índice no próprio objeto/item! :c)
itemA.HeapIndex=itemB.HeapIndex;
itemB.HeapIndex=itemA_HeapIndex;
}
public void Clear(){
Array.Clear(items,0,items.Length);
currentItemsCount=0;
}
}
public interface IHeapItem<T>:IComparable<T>{
int HeapIndex{get;set;}
}