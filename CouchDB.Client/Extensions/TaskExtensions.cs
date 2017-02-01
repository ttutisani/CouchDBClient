using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CouchDB.Client
{
    /// <summary>
    /// Defines extension methods over <see cref="Task"/> and <see cref="Task{TResult}"/>.
    /// </summary>
    internal static class TaskExtensions
    {
        /// <summary>
        /// Configures awaitable task so that it does not have to continue on the currently captured context.
        /// </summary>
        /// <param name="task">Instance of <see cref="Task"/> type.</param>
        /// <returns>Instance of <see cref="ConfiguredTaskAwaitable"/> type.</returns>
        public static ConfiguredTaskAwaitable Safe(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// Configures awaitable task so that it does not have to continue on the currently captured context.
        /// </summary>
        /// <typeparam name="TResult">Type of result that the given task returns.</typeparam>
        /// <param name="task">Instance of <see cref="Task{TResult}"/> type.</param>
        /// <returns>Instance of <see cref="ConfiguredTaskAwaitable{TResult}"/> type.</returns>
        public static ConfiguredTaskAwaitable<TResult> Safe<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}
