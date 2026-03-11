//可交互对象抽象
public interface IInteractable
{
    string GetInteractHint();//获取交互提示文本
    void Interact();//执行交互逻辑的方法
}
