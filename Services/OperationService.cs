using RealtYeahBackend.Entities;
using RealtYeahBackend.Models.Constants;
using Microsoft.EntityFrameworkCore;

namespace RealtYeahBackend.Services
{
    public class OperationService
    {
        private readonly RealtyeahContext context;

        public OperationService(RealtyeahContext context)
        {
            this.context = context;
        }

        public Operation AddAct(int operationID, string actType)
        {
            Operation operation = context.Operations.Where(operation => operation.OperationId == operationID)
                .Include(operation => operation.CounteragentLeadNavigation)
                .ThenInclude(counteragent => counteragent.ClientsStatusesAssignments)
                .ThenInclude(status => status.Operation)
                .Include(operation => operation.CounteragentSecondaryNavigation)
                .ThenInclude(counteragent => counteragent.ClientsStatusesAssignments)
                .ThenInclude(status => status.Operation)
                .Include(operation => operation.EstateObjectNavigation)
                .FirstOrDefault();

            if (operation == null)
            {
                throw new ArgumentException("No operation with such ID");
            } 
            else if (!string.IsNullOrWhiteSpace(operation.ActType))
            {
                throw new ArgumentException("Act has been already assigned to this operation");
            }

            string counteragentLeadStatus = "";
            string counteragentSecondaryStatus = "";
            string objectStatus = "";
            if (ActTypesConst.BuyAgentDeal.Equals(actType))
            {
                operation = RemoveArchiveStatuses(operation);

                counteragentLeadStatus = context.ClientsStatuses.Find("Покупець").Status;
            } 
            else if (ActTypesConst.SellAgentDeal.Equals(actType))
            {
                operation = RemoveArchiveStatuses(operation);

                counteragentLeadStatus = context.ClientsStatuses.Find("Продавець").Status;
                objectStatus = context.ObjectsStatuses.Find("Виставлений на продаж").Status;
            }
            else if (ActTypesConst.RentAgentDeal.Equals(actType))
            {
                operation = RemoveArchiveStatuses(operation);

                counteragentLeadStatus = context.ClientsStatuses.Find("Орендар").Status;
            } 
            else if (ActTypesConst.ForRentAgentDeal.Equals(actType))
            {
                operation = RemoveArchiveStatuses(operation);

                counteragentLeadStatus = context.ClientsStatuses.Find("Орендодавець").Status;
                objectStatus = context.ObjectsStatuses.Find("Виставлений на оренду").Status;
            } 
            else if (ActTypesConst.Pledge.Equals(actType))
            {
                objectStatus = context.ObjectsStatuses.Find("Зарезервований").Status;
            } 
            else if (ActTypesConst.FinalDeal.Equals(actType))
            {
                objectStatus = context.ObjectsStatuses.Find("Архівований").Status;
                
                ClientsStatusesAssignment leadStatusAssignment = operation.CounteragentLeadNavigation.ClientsStatusesAssignments
                                                                          .Where(status => status.Operation.EstateObjectId == operation.EstateObjectId)
                                                                          .FirstOrDefault();
                ClientsStatusesAssignment secondaryStatusAssignment = operation.CounteragentSecondaryNavigation.ClientsStatusesAssignments
                                                                               .Where(status => ActTypesConst.BuyAgentDeal.Equals(status.Operation.ActType)
                                                                                             || ActTypesConst.RentAgentDeal.Equals(status.Operation.ActType))
                                                                               .FirstOrDefault();
                operation.CounteragentLeadNavigation.ClientsStatusesAssignments.Remove(leadStatusAssignment);
                operation.CounteragentSecondaryNavigation.ClientsStatusesAssignments.Remove(secondaryStatusAssignment);

                if (operation.CounteragentLeadNavigation.ClientsStatusesAssignments.Count <= 0)
                {
                    counteragentLeadStatus = "Минулий клієнт";
                }
                if (operation.CounteragentSecondaryNavigation.ClientsStatusesAssignments.Count <= 0)
                {
                    counteragentSecondaryStatus = "Минулий клієнт";
                }
            }

            if (string.IsNullOrWhiteSpace(counteragentLeadStatus)
                && string.IsNullOrWhiteSpace(counteragentSecondaryStatus)
                && string.IsNullOrWhiteSpace(objectStatus)
                && !ActTypesConst.ObjectCheck.Equals(actType))
            {
                throw new ArgumentException("Attempt to assign a non-existing act type \"" + actType + "\"");
            }

            operation.ActType = actType;
            if (!string.IsNullOrWhiteSpace(counteragentLeadStatus))
            {
                operation.ClientsStatusesAssignments.Add(new ClientsStatusesAssignment
                {
                    ClientId = operation.CounteragentLead,
                    Status = counteragentLeadStatus,
                    OperationId = operation.OperationId
                });
            }
            if (!string.IsNullOrWhiteSpace(counteragentSecondaryStatus))
            {
                operation.ClientsStatusesAssignments.Add(new ClientsStatusesAssignment
                {
                    ClientId = operation.CounteragentSecondary.Value,
                    Status = counteragentSecondaryStatus,
                    OperationId = operation.OperationId
                });
            }
            if (!string.IsNullOrWhiteSpace(objectStatus))
            {
                operation.EstateObjectNavigation.Status = objectStatus;
            }

            operation.Status = "Успішно";

            context.SaveChanges();

            return operation;
        }

        public Operation CancelOperation(int operationID)
        {
            Operation operation = context.Operations.Where(operation => operation.OperationId == operationID)
                .Include(operation => operation.EstateObjectNavigation)
                .Include(operation => operation.CounteragentLeadNavigation)
                .ThenInclude(counteragent => counteragent.ClientsStatusesAssignments)
                .FirstOrDefault();

            if (operation == null)
            {
                throw new ArgumentException("No operation with such ID");
            } 
            else if (string.IsNullOrWhiteSpace(operation.ActType))
            {
                operation.Status = context.OperationsStatuses.Find("Неуспішно").Status;
            } 
            else
            {
                if (ActTypesConst.Pledge.Equals(operation.ActType))
                {
                    Operation backupOperation = context.Operations.Where(op => op.CounteragentLead == operation.CounteragentLead
                                                                            && op.EstateObjectId == operation.EstateObjectId
                                                                            && !(ActTypesConst.Pledge.Equals(op.ActType) || ActTypesConst.FinalDeal.Equals(op.ActType)))
                                                                  .OrderByDescending(op => op.Date)
                                                                  .Include(op => op.EstateObjectNavigation)
                                                                  .FirstOrDefault();
                    
                    if (backupOperation == null)
                    {
                        throw new NullReferenceException("Can't find any suitable backup operation");
                    }

                    operation.EstateObjectNavigation.Status = backupOperation.EstateObjectNavigation.Status;
                }
                else if (ActTypesConst.BuyAgentDeal.Equals(operation.ActType)
                        || ActTypesConst.SellAgentDeal.Equals(operation.ActType)
                        || ActTypesConst.RentAgentDeal.Equals(operation.ActType)
                        || ActTypesConst.ForRentAgentDeal.Equals(operation.ActType))
                {                  
                    operation.EstateObjectNavigation.Status = context.ObjectsStatuses.Find("Архівований").Status;
                    ClientsStatusesAssignment status = operation.CounteragentLeadNavigation.ClientsStatusesAssignments
                                                       .Where(status => status.OperationId == operation.OperationId)
                                                       .FirstOrDefault();
                    operation.CounteragentLeadNavigation.ClientsStatusesAssignments.Remove(status);

                    if (operation.CounteragentLeadNavigation.ClientsStatusesAssignments.Count <= 0)
                    {
                        operation.CounteragentLeadNavigation.ClientsStatusesAssignments.Add(new ClientsStatusesAssignment { ClientId = operation.CounteragentLead,
                                                                                                                            Status = context.ClientsStatuses.Find("Минулий клієнт").Status,
                                                                                                                            OperationId = operation.OperationId });
                    }
                }

                operation.Status = context.OperationsStatuses.Find("Неуспішно").Status;
            }

            context.SaveChanges();

            return operation;
        }

        private Operation RemoveArchiveStatuses(Operation operation)
        {
            List<ClientsStatusesAssignment> leadStatusAssignments = operation.CounteragentLeadNavigation.ClientsStatusesAssignments
                .Where(status => status.Status.Equals("Внесений у базу")
                              || status.Status.Equals("Минулий клієнт")).ToList();

            if (leadStatusAssignments != null)
            {
                foreach (ClientsStatusesAssignment assignment in leadStatusAssignments)
                {
                    operation.CounteragentLeadNavigation.ClientsStatusesAssignments.Remove(assignment);
                }
            }

            return operation;
        }
     
        public List<bool> GetAvailableActs(Operation operation)
        {
            List<bool> availableActs = new List<bool> { false, false, false, false, false, false, false, false, false };

            if (ActTypesConst.FinalDeal.Equals(operation.ActType))
            {
                return availableActs;
            }

            if ((operation.InverseFkOperationLeadNavigation == null || operation.InverseFkOperationLeadNavigation.Count == 0)
                 && (operation.InverseFkOperationSecondaryNavigation == null || operation.InverseFkOperationSecondaryNavigation.Count == 0))
            {
                availableActs[7] = !operation.Status.Equals("Неуспішно");
                
                availableActs[8] = true;
            } 
            else if (operation.InverseFkOperationLeadNavigation != null
                     && operation.InverseFkOperationLeadNavigation.Where(op => op.Status.Equals("Неуспішно")).Count() == operation.InverseFkOperationLeadNavigation.Count
                     && operation.InverseFkOperationSecondaryNavigation != null
                     && operation.InverseFkOperationSecondaryNavigation.Where(op => op.Status.Equals("Неуспішно")).Count() == operation.InverseFkOperationSecondaryNavigation.Count)
            {
                availableActs[7] = !operation.Status.Equals("Неуспішно");
            }

            if (string.IsNullOrWhiteSpace(operation.ActType))
            {
                if (operation.FkOperationLeadNavigation == null && operation.CounteragentSecondary == null)
                {
                    availableActs[0] = true;
                    availableActs[1] = true;
                    availableActs[2] = true;
                    availableActs[3] = true;
                } 

                if (operation.FkOperationLeadNavigation != null && operation.FkOperationSecondaryNavigation != null 
                    && (operation.InverseFkOperationLeadNavigation == null 
                        || operation.InverseFkOperationLeadNavigation.Count == 0 
                        || operation.InverseFkOperationLeadNavigation.Where(op => op.Status.Equals("Неуспішно")).Count() == operation.InverseFkOperationLeadNavigation.Count)
                    && (operation.InverseFkOperationSecondaryNavigation == null 
                        || operation.InverseFkOperationSecondaryNavigation.Count == 0 
                        || operation.InverseFkOperationSecondaryNavigation.Where(op => op.Status.Equals("Неуспішно")).Count() == operation.InverseFkOperationSecondaryNavigation.Count))
                {
                    if ((ActTypesConst.SellAgentDeal.Equals(operation.FkOperationLeadNavigation.ActType)
                         && ActTypesConst.BuyAgentDeal.Equals(operation.FkOperationSecondaryNavigation.ActType))
                        || (ActTypesConst.ForRentAgentDeal.Equals(operation.FkOperationLeadNavigation.ActType)
                            && ActTypesConst.RentAgentDeal.Equals(operation.FkOperationSecondaryNavigation.ActType)))
                    {
                        availableActs[4] = true;
                    }
                    else if (ActTypesConst.ObjectCheck.Equals(operation.FkOperationLeadNavigation.ActType)
                             && ActTypesConst.ObjectCheck.Equals(operation.FkOperationSecondaryNavigation.ActType))
                    {
                        availableActs[5] = true;
                    } 
                    else if (ActTypesConst.Pledge.Equals(operation.FkOperationLeadNavigation.ActType)
                             && ActTypesConst.Pledge.Equals(operation.FkOperationSecondaryNavigation.ActType))
                    {
                        availableActs[6] = true;
                    }
                }              
            }         

            return availableActs;
        }

        public bool GetNextAvailability(Operation operation)
        {
            if (string.IsNullOrWhiteSpace(operation.ActType))
            {
                return false;
            }
            else if ((ActTypesConst.SellAgentDeal.Equals(operation.ActType) || ActTypesConst.ForRentAgentDeal.Equals(operation.ActType)) 
                && operation.Status.Equals("Успішно"))
            {
                return TravelOperationTree(operation, operation.ActType);
            }
            else if (ActTypesConst.BuyAgentDeal.Equals(operation.ActType) || ActTypesConst.RentAgentDeal.Equals(operation.ActType)
                     && operation.Status.Equals("Успішно"))
            {
                return TravelOperationTreeSecondary(operation, 1, operation.ActType);
            }
            else
            {
                Operation leadOperation = operation;

                while (leadOperation.FkOperationLead != null)
                {
                    context.Entry(leadOperation)
                        .Reference(o => o.FkOperationLeadNavigation)
                        .Load();
                    leadOperation = leadOperation.FkOperationLeadNavigation;
                };

                Operation secondaryOperation = operation;

                while (secondaryOperation.FkOperationSecondary != null)
                {
                    context.Entry(secondaryOperation)
                        .Reference(o => o.FkOperationSecondaryNavigation)
                        .Load();
                    secondaryOperation = secondaryOperation.FkOperationSecondaryNavigation;
                };

                return TravelOperationTree(leadOperation, operation.ActType) && TravelOperationTreeSecondary(secondaryOperation, 1, operation.ActType);
            }          
        }      

        private bool TravelOperationTree(Operation operation, string initialAct)
        {          
            context.Entry(operation)
                .Collection(o => o.InverseFkOperationLeadNavigation)
                .Load();

            foreach (Operation op in operation.InverseFkOperationLeadNavigation)
            {
                if (ActTypesConst.Pledge.Equals(op.ActType) && op.Status.Equals("Успішно") 
                    && !ActTypesConst.Pledge.Equals(initialAct))
                {
                    return false;
                }
            }

            foreach (Operation op in operation.InverseFkOperationLeadNavigation)
            {
                return TravelOperationTree(op, initialAct);
            }

            return true;
        }

        private bool TravelOperationTreeSecondary(Operation operation, int level, string initialAct)
        {           
            List<Operation> operations = new List<Operation>();

            if (level > 1)
            {
                context.Entry(operation)
                    .Collection(o => o.InverseFkOperationSecondaryNavigation)
                    .Load();

                operations = operation.InverseFkOperationSecondaryNavigation.ToList();
            } 
            else
            {
                context.Entry(operation)
                    .Collection(o => o.InverseFkOperationLeadNavigation)
                    .Load();

                operations = operation.InverseFkOperationLeadNavigation.ToList();
            }

            foreach (Operation op in operations)
            {
                if (ActTypesConst.Pledge.Equals(op.ActType) && op.Status.Equals("Успішно")
                    && !ActTypesConst.Pledge.Equals(initialAct))
                {
                    return false;
                }
            }

            foreach (Operation op in operations)
            {
                level += 1;
                return TravelOperationTreeSecondary(op, level, initialAct);
            }

            return true;
        }
    }
}
