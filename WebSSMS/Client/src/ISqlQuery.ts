import { ISqlQuery } from '@/ISqlQuery';
export enum SqlQueryStatus {
  Running,
  Finished,
  Error,
  Cancelled
}
console.log(SqlQueryStatus)

export interface ISqlQuery {
    id: string,
		ConnectionName: string,
		ConnectionId: string,
		SqlText: string,
		QueryStatus: SqlQueryStatus,
		Stats: {[k: string]: any}
		data: {}[];
		Error: string
		NextQuery: ISqlQuery
}