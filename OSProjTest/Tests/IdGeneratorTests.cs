using OSProj.Generator;

namespace OSProjTest.Tests
{
  public class IdGeneratorTests
  {
    [Fact]
    public void IdGenerator_ShouldGenerateSequentialIds()
    {
      int id1 = IdGenerator.Id;
      int id2 = IdGenerator.Id;

      Assert.Equal(id1 + 1, id2);
    }
  }
}
