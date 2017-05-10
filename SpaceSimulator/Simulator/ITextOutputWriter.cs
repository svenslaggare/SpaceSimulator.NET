using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceSimulator.Simulator
{
    /// <summary>
    /// Represents an interface for writing text output
    /// </summary>
    public interface ITextOutputWriter
    {
        /// <summary>
        /// Writes the given line to the output
        /// </summary>
        /// <param name="line">line</param>
        void WriteLine(string line);
    }

    /// <summary>
    /// Contains extension methods for the <see cref="ITextOutputWriter"/>
    /// </summary>
    public static class ITextOutputWriterExtensions
    {
        /// <summary>
        /// Writes the given line to the output
        /// </summary>
        /// <param name="textOutputWriter">The text output writer</param>
        /// <param name="name">The name of the object to write for</param>
        /// <param name="line">The line</param>
        public static void WriteLine(this ITextOutputWriter textOutputWriter, string name, string line)
        {
            if (textOutputWriter is NullTextOutputWriter)
            {
                return;
            }

            textOutputWriter.WriteLine($"{name}: {line}");
        }
    }

    /// <summary>
    /// Represents a null text output writer
    /// </summary>
    public sealed class NullTextOutputWriter : ITextOutputWriter
    {
        /// <summary>
        /// Writes the given line to the output
        /// </summary>
        /// <param name="line">line</param>
        public void WriteLine(string line)
        {
            
        }
    }

    /// <summary>
    /// Represents a text output writer using the console
    /// </summary>
    public sealed class ConsoleTextOutputWriter : ITextOutputWriter
    {
        /// <summary>
        /// Writes the given line to the output
        /// </summary>
        /// <param name="line">line</param>
        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }
}
