from fastapi import FastAPI
from pydantic import BaseModel
import random

# Initialize the API
app = FastAPI()

# Define the data shape we expect to receive
class Transaction(BaseModel):
    TransactionId: str
    Amount: float
    MerchantId: str
    Timestamp: str

# Define the response shape
class FraudPrediction(BaseModel):
    IsFraud: bool
    RiskScore: float
    Reason: str

@app.post("/predict", response_model=FraudPrediction)
async def predict_fraud(transaction: Transaction):
    print(f"Analyzing transaction: {transaction.TransactionId} for ${transaction.Amount}")

    # SIMULATION LOGIC (Replace this with actual ML model later)
    # Rule 1: Amounts over 10,000 are 95% likely to be fraud
    if transaction.Amount > 10000:
        return FraudPrediction(
            IsFraud=True, 
            RiskScore=0.95, 
            Reason="Amount exceeds threshold"
        )
    
    # Rule 2: Random low risk for everything else
    # We return a float between 0.0 and 0.1
    return FraudPrediction(
        IsFraud=False, 
        RiskScore=0.05, 
        Reason="Normal transaction pattern"
    )

@app.get("/health")
def health_check():
    return {"status": "active", "model_version": "1.0.0"}