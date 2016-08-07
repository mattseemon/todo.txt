using Seemon.Todo.Extensions;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Seemon.Todo.Models
{
    public class TaskList : IEnableLogger
    {
        string _filePath = string.Empty;
        string _preferredLineEnding = string.Empty;

        public List<Task> Tasks { get; private set; }

        public List<string> Projects { get; private set; }
        public List<string> Contexts { get; private set; }
        public List<string> Priorities { get; private set; }
        public bool PreserverWhiteSpace { get; set; }

        public TaskList(string filePath, bool preserveWhiteSpace = false)
        {
            this._filePath = filePath;
            this._preferredLineEnding = Environment.NewLine;
            this.PreserverWhiteSpace = preserveWhiteSpace;

            this.ReloadTasks();
        }

        public void Add(Task task)
        {
            try
            {
                var output = task.ToString();

                this.Log().Debug("Adding task '{0}'", output);

                var text = File.ReadAllText(_filePath);
                if (text.Length > 0 && !text.EndsWith(_preferredLineEnding))
                    output = _preferredLineEnding + output;

                File.AppendAllLines(_filePath, new string[] { output });

                Tasks.Add(task);

                this.Log().Debug("Task '{0}' added", output);
            }
            catch(Exception ex)
            {
                var msg = "An error occurred while trying to add your task to your task list file";
                this.Log().ErrorException(msg, ex);
                throw new TaskException(msg, ex);
            }
            finally
            {
                UpdateTaskListMetaData();
            }
        }

        public void Delete(Task task)
        {
            try
            {
                this.Log().Debug("Deleting task '{0}'", task.ToString());

                if (Tasks.Remove(Tasks.First(t => t.Raw == task.Raw)))
                    WriteAllTasksToFile();

                this.Log().Debug("Task '{0}' deleted", task.ToString());
            }
            catch(IOException ex)
            {
                var msg = "An error occured while trying to remove your task from the task list file.";
                this.Log().ErrorException(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch(Exception ex)
            {
                this.Log().Error(ex.ToString());
                throw;
            }
            finally
            {
                UpdateTaskListMetaData();
            }
        }

        public void UpdateTask(Task currentTask, Task newTask, bool writeTasks = true)
        {
            this.Log().Debug("Updating task '{0}' to '{1}'.", currentTask.ToString(), newTask.ToString());

            try
            {
                if (!Tasks.Any(t => t.Raw == currentTask.Raw))
                    throw new Exception("Task does not exists in the todo.txt file.");

                var currentIndex = Tasks.IndexOf(Tasks.First(t => t.Raw == currentTask.Raw));

                Tasks[currentIndex] = newTask;

                this.Log().Debug("Task '{0}' updated", currentTask.ToString());

                if (writeTasks)
                    WriteAllTasksToFile();
            }
            catch(Exception ex)
            {
                var msg = "An error occurred while trying to update your task in the task list file";
                this.Log().ErrorException(msg, ex);
                throw new TaskException(msg, ex);
            }
            finally
            {
                UpdateTaskListMetaData();
            }
        }

        private void WriteAllTasksToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_filePath))
                {
                    writer.NewLine = _preferredLineEnding;
                    foreach (Task t in Tasks)
                        writer.WriteLine(t.ToString());
                    writer.Close();
                }
            }
            catch(IOException ex)
            {
                var msg = "An error occurred while trying to write the task list file";
                this.Log().ErrorException(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch(Exception ex)
            {
                this.Log().Error(ex.ToString());
                throw;
            }
        }

        public void ReloadTasks()
        {
            this.Log().Info("Loading tasks from {0}.", _filePath);

            try
            {
                this.Tasks = new List<Task>();

                var file = File.OpenRead(_filePath);
                using (var reader = new StreamReader(file))
                {
                    string raw;
                    while((raw = reader.ReadLine()) != null)
                    {
                        if (!raw.IsNullOrEmpty() || PreserverWhiteSpace)
                            this.Tasks.Add(new Task(raw));
                    }
                }

                this.Log().Info("Finished loading tasks from {0}", _filePath);
                this._preferredLineEnding = this.GetPreferredFileEndingFromFile();
            }
            catch(IOException ex)
            {
                var message = "There was a problem trying to read from your todo.txt file.";
                this.Log().ErrorException(message, ex);
                throw new TaskException(message, ex);
            }
            catch(Exception ex)
            {
                this.Log().Error(ex.ToString());
                throw;
            }
            finally
            {
                this.UpdateTaskListMetaData();
            }
        }

        public void UpdateTaskListMetaData()
        {
            var uniqueProjects = new SortedSet<string>();
            var uniqueContexts = new SortedSet<string>();
            var uniquePriorities = new SortedSet<string>();

            foreach(Task t in Tasks)
            {
                foreach(string p in t.Projects)
                    uniqueProjects.Add(p);

                foreach(string c in t.Contexts)
                    uniqueContexts.Add(c);

                uniquePriorities.Add(t.Priority);
            }

            this.Projects = uniqueProjects.ToList<string>();
            this.Contexts = uniqueContexts.ToList<string>();
            this.Priorities = uniquePriorities.ToList<string>();
        }

        protected string GetPreferredFileEndingFromFile()
        {
            try
            {
                using (StreamReader fileStream = new StreamReader(_filePath))
                {
                    char previousChar = '\0';

                    // Read the first 4000 characters to try and find a newline
                    for(int i = 0; i < 4000; i++)
                    {
                        int b = fileStream.Read();
                        if (b == -1) break;

                        char currentChar = (char)b;
                        if (currentChar == '\n')
                            return (previousChar == '\r') ? "\r\n" : "\n";

                        previousChar = currentChar;
                    }

                    // if no newline found, use the default newline character for the environment
                    return Environment.NewLine;
                }
            }
            catch(IOException ex)
            {
                var msg = "An error occurred while trying to read the task list file";
                this.Log().ErrorException(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch(Exception ex)
            {
                this.Log().Error(ex.ToString());
                throw;
            }
        }
    }
}