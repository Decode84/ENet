using System.Net.Sockets;

namespace ENet.Core;

public class ConnectionPool
{
    // List to store the connections in the pool
    private readonly List<Connection> _connections = new();
    
    // Maximum size of the pool
    private readonly int _maxSize;
    
    public double Timeout { get; set; } = 60; // default timeout is 60 seconds
    
    // Timeout value for removing stale connections
    private readonly int _timeout;

    // Constructor to initialize the connection pool
    public ConnectionPool(int maxSize, int timeout)
    {
        // Set the max size of the pool
        _maxSize = maxSize;
        
        // Set the timeout for removing stale connections
        _timeout = timeout;
    }

    /// <summary>
    /// Get a connection from the pool
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Connection GetConnection(Socket socket)
    {
        // Lock the connections list for synchronization
        lock (_connections)
        {
            // If the number of connections in the pool is equal to or greater than the max size, throw an exception
            if(_connections.Count >= _maxSize)
                throw new Exception("Connection pool is full");

            // If there are connections available in the pool, remove and return the first one
            if (_connections.Count > 0)
            {
                var con = _connections[0];
                _connections.RemoveAt(0);
                return con;
            }
        }

        // If there are no connections available in the pool, create a new connection
        return new Connection(socket);
    }
    
    /// <summary>
    /// Return a connection to the pool
    /// </summary>
    /// <param name="connection"></param>
    public void ReturnConnection(Connection connection)
    {
        // Lock the connections list for synchronization
        lock (_connections)
        {
            // If the number of connections in the pool is equal to or greater than the max size, return without adding the connection
            if (_connections.Count >= _maxSize)
                return;

            // Add the connection back to the pool
            _connections.Add(connection);
        }
    }

    /// <summary>
    /// Remove stale connections from the pool
    /// </summary>
    public void RemoveStaleConnection()
    {
        // Lock the connections list for synchronization
        lock (_connections)
        {
            // Get the current time
            var now = DateTime.Now;
            
            // Remove all the connections that are stale (i.e., have not been updated for more than the timeout)
            _connections.RemoveAll(c => (now - c.LastUpdate).TotalSeconds > _timeout);
        }
    }
    
    
}