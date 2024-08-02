

using ecs_gen.Generation;

namespace ecs_gen;

class Program
{
  static void Main(string[] args)
  {
    string schemaPath = args[0];
    string outputPath = args[1];

    Generator.Gen(schemaPath, outputPath);
  }
}