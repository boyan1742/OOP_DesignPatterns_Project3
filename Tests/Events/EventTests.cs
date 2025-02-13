using OOP_DesignPatterns_Project3.Events;

namespace Tests.Events;

public class EventTests
{
    private bool m_invoke1 = false;
    private bool m_invoke2 = false;
    private bool m_invoke3 = false;

    [Fact]
    public void TestBind()
    {
        Assert.True(EventMaster.Bind("testBind", new EventListener("testBind", e => { })));
    }

    [Fact]
    public void TestBindSameID()
    {
        Assert.True(EventMaster.Bind("TestBindSameID", new EventListener("TestBindSameID", e => { })));
        Assert.False(EventMaster.Bind("TestBindSameID", new EventListener("TestBindSameID", e => { })));
    }

    [Fact]
    public void TestInvoke()
    {
        Assert.True(EventMaster.Bind("TestInvoke", new EventListener("TestInvoke", _ => m_invoke1 = true)));
        
        EventMaster.Invoke("TestInvoke", new EmptyEvent());

        Task.Delay(1000); //wait 1 second

        Assert.True(m_invoke1);
    }
    
    [Fact]
    public void TestInvokeMultiple()
    {
        Assert.True(EventMaster.Bind("TestInvokeMultiple", new EventListener("TestInvokeMultiple1", _ => m_invoke2 = true)));
        Assert.True(EventMaster.Bind("TestInvokeMultiple", new EventListener("TestInvokeMultiple2", _ => m_invoke3 = true)));
        
        EventMaster.Invoke("TestInvokeMultiple", new EmptyEvent());

        Task.Delay(1000); //wait 1 second

        Assert.True(m_invoke2);
        Assert.True(m_invoke3);
    }
}