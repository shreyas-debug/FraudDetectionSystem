import { useEffect, useState } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import 'bootstrap/dist/css/bootstrap.min.css';

function App() {
  const [transactions, setTransactions] = useState([]);
  
  useEffect(() => {
    // Connect to the .NET Backend Hub
    const connection = new HubConnectionBuilder()
      .withUrl("http://localhost:5000/fraudHub") 
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => {
        console.log('Connected to SignalR');
        
        // Listen for the specific event name from C#
        connection.on('ReceiveFraudAlert', (tx) => {
          setTransactions(prev => [tx, ...prev].slice(0, 15));
        });
      })
      .catch(e => console.log('Connection failed: ', e));
  }, []);

  return (
    <div className="container mt-5">
      <div className="row mb-4">
        <div className="col">
          <h1>🛡️ Financial Fraud Monitor</h1>
          <p className="text-muted">Live Stream via Kafka & SignalR</p>
        </div>
      </div>

      <div className="card shadow-sm">
        <div className="card-header bg-dark text-white d-flex justify-content-between">
          <span>Live Transactions</span>
          <span className="badge bg-light text-dark">{transactions.length} items</span>
        </div>
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead>
              <tr>
                <th>Time</th>
                <th>Transaction ID</th>
                <th>Merchant</th>
                <th>Amount</th>
                <th>Risk Score</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map((tx, i) => (
                <tr key={i} className={tx.isFraud ? "table-danger" : ""}>
                  <td>{new Date(tx.timestamp).toLocaleTimeString()}</td>
                  <td>{tx.transactionId}</td>
                  <td>{tx.merchantId}</td>
                  <td>${tx.amount}</td>
                  <td>
                    <div className="progress" style={{height: '20px'}}>
                      <div 
                        className={`progress-bar ${tx.isFraud ? 'bg-danger' : 'bg-success'}`} 
                        style={{width: `${tx.riskScore * 100}%`}}
                      >
                        {(tx.riskScore * 100).toFixed(0)}%
                      </div>
                    </div>
                  </td>
                  <td>
                    {tx.isFraud ? 
                      <strong className="text-danger">FRAUD</strong> : 
                      <span className="text-success">CLEAN</span>
                    }
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default App;